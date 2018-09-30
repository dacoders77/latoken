using BitMEXAssistant;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HomeTask.Tests {
    [TestClass]
    public class BitmexRealtimeDataServiceTests {
        [TestMethod]
        public void Initialize_ShouldSubscribeOnOrder() {
            // Arrange
            var testDataService = new TestDataService();
            var testWebSocket = (TestWebSocket) testDataService.WebSocket;
            var symbol = "test";
            var service = new BitmexRealtimeDataService(testDataService, symbol);

            // Act
            service.Initialize();

            // Assert
            Assert.IsTrue(testWebSocket.LastData.Contains(@"{""op"": ""subscribe"", ""args"": [""order""]}"));
            Assert.AreEqual(@"{""op"": ""subscribe"", ""args"": [""order""]}", testWebSocket.LastData[0]);
        }

        [TestMethod]
        public void OnSocketTradeMessage_ShouldRaiseTradeReceivedEvent() {
            // Arrange
            var testDataService = new TestDataService();
            var testWebSocket = (TestWebSocket)testDataService.WebSocket;
            var symbol = "test";
            var service = new BitmexRealtimeDataService(testDataService, symbol);
            service.Initialize();

            // Act
            testWebSocket.RaiseOnMessage("");
            
            // Assert
        }
    }
}