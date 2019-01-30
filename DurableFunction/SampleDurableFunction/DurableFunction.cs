using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AzureFunctions.Autofac;
using Common;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace SampleDurableFunction
{
    [DependencyInjectionConfig(typeof(DIConfig))]
    public static class DurableFunction
    {
        [FunctionName("Orchestrator")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context,
            ILogger log)
        {
            if (!context.IsReplaying)
            {
                log.LogInformation("Orchestrator Triggerd");
            }
            var outputs = new List<string>();
            var input = context.GetInput<User>();

            try
            {
                using (var token = new CancellationTokenSource())
                {
                    var dueIn = context.CurrentUtcDateTime.AddMinutes(10);
                    var timer = context.CreateTimer(dueIn, token.Token);

                    var approvalEvent = context.WaitForExternalEvent("Activate");
                    if (approvalEvent == await Task.WhenAny(timer, approvalEvent))
                    {
                        outputs.Add(await context.CallActivityAsync<string>("AddBlob", input));
                        outputs.Add(await context.CallActivityAsync<string>("AddTable", input));
                    }
                    else
                    {
                        log.LogInformation("Invalid Email");
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogInformation($"Exception occuerd:{ex.Message}");
            }
            log.LogInformation("Orchestrator Completed");
            return outputs;
        }

        [FunctionName("AddBlob")]
        public static async Task<string> AddBlob([ActivityTrigger]User user,
            IBinder binder,
            ILogger log)
        {
            log.LogInformation("Blob Insertion Triggerd");
            var cloudBlockBlob = binder.Bind<CloudBlockBlob>(new BlobAttribute($"myblobitems/{user.Name}"));
            var stringify = JsonConvert.SerializeObject(user);
            await cloudBlockBlob.UploadTextAsync(stringify);

            log.LogInformation("Blob Insertion Triggerd");
            return $"myblobitems/{user.Name} Uploaded";
        }

        [FunctionName("AddTable")]
        public static string AddTable(
            [ActivityTrigger]User user,
            ILogger log,
            [Table("mytableitems")]out UserEntity tableEntity)
        {
            log.LogInformation("Table Insertion Triggerd");
            tableEntity = new UserEntity()
            {
                Name = user.Name,
                Email = user.Email,
                Age = user.Age,
                Sex = user.Sex,

                PartitionKey = DateTime.Now.Year.ToString(),
                RowKey = Guid.NewGuid().ToString()
            };
            log.LogInformation("Table Insertion Requested");
            return $"Entry With Name {user.Name} Inserted To Table";
        }

        [FunctionName("DurableStart")]
        public static async Task<HttpResponseMessage> DurableStart(
            [HttpTrigger(AuthorizationLevel.Function, "POST", null)]HttpRequestMessage request,
            [OrchestrationClient]DurableOrchestrationClientBase starter,
             [Inject]IUserAgeValidator userAgeValidator,
            ILogger log)
        {
            log.LogInformation("Durable Function Triggerd");
            
            try
            {
                var requestBody = await request.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<User>(requestBody);
                if(!userAgeValidator.IsValid(user))
                {
                    return request.CreateResponse(HttpStatusCode.Forbidden);
                }
                string instanceId = await starter.StartNewAsync("Orchestrator", user);
                log.LogInformation($"Started Orchestrator With ID = '{instanceId}'.");
                return starter.CreateCheckStatusResponse(request, instanceId);
            }
            catch (Exception ex)
            {
                log.LogInformation($"Exception occuerd:{ex.Message}");
            }
            finally
            {
                log.LogInformation("Durable Function Completed");
            }
            return request.CreateResponse(HttpStatusCode.InternalServerError);
        }

        [FunctionName("DurableExternalEvent")]
        public static async Task RaiseExternalEvent(
            [HttpTrigger(AuthorizationLevel.Function, "POST", null)]HttpRequestMessage request,
            [OrchestrationClient]DurableOrchestrationClient client,
            ILogger log)
        {
            var requestBody = await request.Content.ReadAsStringAsync();
            var instanceId = JsonConvert.DeserializeObject<string>(requestBody);
            try
            {
                log.LogInformation("Durable External Event Triggerd");
                await client.RaiseEventAsync(instanceId, "Activate");
            }
            catch (Exception ex)
            {
                log.LogInformation($"Exception occuerd:{ex.Message}");
            }
            finally
            {
                log.LogInformation("Durable External Event Completed");
            }
        }
    }
}