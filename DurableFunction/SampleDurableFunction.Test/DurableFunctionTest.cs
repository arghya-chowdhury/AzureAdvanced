using Common;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SampleDurableFunction.Test
{
    [TestFixture]
    public class DurableFunctionTest
    {
        [Test]
        public void TriggerDurableStartInvalidUser()
        {
            //Arrange
            var reqMessage = new HttpRequestMessage();
            var user = new User() { Name = "TestUser", Age = 17, Email = "testuser@gmail.com", Sex = Sex.Male };
            var stringify = JsonConvert.SerializeObject(user);
            reqMessage.Content = new StringContent(stringify, Encoding.UTF8, "application/json");
            var orchestratorClient = new Mock<DurableOrchestrationClientBase>();
            var mocklogger = new Mock<ILogger>();
            var validator = new UserAgeValidator();

            //Act
            var responseMessage = DurableFunction.DurableStart(reqMessage, orchestratorClient.Object, validator, mocklogger.Object).GetAwaiter().GetResult();

            //Assert
            Assert.True(responseMessage.StatusCode==HttpStatusCode.Forbidden);
        }

        [Test]
        public void TriggerDurableStartValidUser()
        {
            //Arrange
            var orchestratorClient = new Mock<DurableOrchestrationClientBase>();
            var guid = Guid.NewGuid().ToString();
            orchestratorClient.Setup(c => c.StartNewAsync(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(Task.FromResult<string>(guid));
            orchestratorClient
                .Setup(x => x.CreateCheckStatusResponse(It.IsAny<HttpRequestMessage>(), It.IsAny<string>()))
                .Returns(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(guid),
                });

            var mocklogger = new Mock<ILogger>();
            var validator = new UserAgeValidator();
            var reqMessage = new HttpRequestMessage();
            var user = new User() { Name = "TestUser", Age = 19, Email = "testuser@gmail.com", Sex = Sex.Male };
            var stringify = JsonConvert.SerializeObject(user);
            reqMessage.Content = new StringContent(stringify, Encoding.UTF8, "application/json");

            //Act
            var responseMessage = DurableFunction.DurableStart(reqMessage, orchestratorClient.Object, validator, mocklogger.Object).GetAwaiter().GetResult();
            var content = responseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            //Assert
            Assert.AreEqual(guid, content);
        }
    }
}
