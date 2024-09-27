using System;
using System.Collections.Generic;
// using System.Text.Json;
using NUnit.Framework;
using StreamArtist.Domain;
using Newtonsoft.Json;

namespace StreamArtist.Tests.Services
{

public abstract class Person
{
    public string Name { get; set; } 
}

public class Employee : Person
{
    public int EmployeeId { get; set; }
}

    [TestFixture] 
    public class EffectsServiceTests
    {
        [Test] // Change from [Test] to [TestMethod]
        public void ProcessChatAnimations_CanSerializeListOfRendererRequests()
        {
            var people = new List<Person> // Changed to List<Person>
        {
            new Employee { Name = "Alice", EmployeeId = 123 },
            new Employee { Name = "Bob", EmployeeId = 456 }
        };

        // var options = new JsonSerializerOptions { WriteIndented = true };
        var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(people);
        //JsonConvert.SerializeObject(people);
        // string jsonString = JsonSerializer.Serialize(people, options);
        Console.WriteLine(jsonString); 

            var request = new FireRendererRequest
                {
                    Amount = 10,
                    Size = 10,
                    Name = "Joel Gerard"
                };
            var list = new List<RendererRequest>
            {
                request
            };

            // Console.WriteLine(JsonSerializer.Serialize(list));
            // Console.WriteLine(JsonSerializer.Serialize(request));

            
        }
    }
}