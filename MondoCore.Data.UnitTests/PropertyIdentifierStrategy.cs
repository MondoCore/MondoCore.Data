using Microsoft.VisualStudio.TestTools.UnitTesting;
using MondoCore.Data.Memory;
using MondoCore.Repository.TestHelper;
using System;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MondoCore.Data.UnitTests
{
    [TestClass]
    [TestCategory("Unit Tests")]
    public class PropertyIdentifierStrategyTests 
    {
        public PropertyIdentifierStrategyTests() 
        {
        }

        [TestMethod]
        public void PropertyIdentifierStrategy_GetId()
        {    
            var strategy = new PropertyIdentifierStrategy<string, Car>("Make");

            var result = strategy.GetId("1234", new Car { Id = "1234", Make = "Chevy", Model = "Corvette" });

            Assert.AreEqual("1234", result.Id);
            Assert.AreEqual("Chevy", result.PartitionKey);

        }
    }

    public class Car : IIdentifiable<string>
    {
        [JsonPropertyName("id")]
        public string      Id    {get; set;}
        public string      id    => Id;

        public string   Make  {get; set;}
        public string   Model {get; set;}
        public string   Color {get; set;}
        public int      Year  {get; set;} = 1964;
    }
}
