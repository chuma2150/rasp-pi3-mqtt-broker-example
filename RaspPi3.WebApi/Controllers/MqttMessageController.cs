﻿using RaspPi3.WebApi.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Http;

namespace RaspPi3.WebApi.Controllers
{
    [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "CC0091")]
    [Authorize]
    public class MqttMessageController : ApiController
    {
        private const int TopCount = 100;

        // GET api/mqttMessage
        /// <summary>
        /// Returns latest 100 saved messages.
        /// </summary>
        /// <returns>A list with up to 100 messages.</returns>
        public IEnumerable<SaveMqttMessageBindingModel> Get()
        {
            using var dbContext = new MqttDbContext();
            var messagesToReturn = dbContext.MqttMessages
                .Take(TopCount)
                .OrderByDescending(m => m.Id)
                .ToList();

            return messagesToReturn;
        }

        // GET api/mqttMessage/Topic
        /// <summary>
        /// Return latest 100 saved messages for a specific topic.
        /// </summary>
        /// <param name="topic">Name of topic.</param>
        /// <returns>A list with up to 100 messages for a topic.</returns>
        public IEnumerable<SaveMqttMessageBindingModel> Get(string topic)
        {
            using var dbContext = new MqttDbContext();
            var messagesToReturn = dbContext.MqttMessages
                .Take(TopCount)
                .Where(m => m.Topic == topic)
                .OrderByDescending(m => m.Id)
                .ToList();

            return messagesToReturn;
        }

        // GET api/mqttMessage/TopicUser
        /// <summary>
        /// Return latest 100 saved messages for a specific topic and user.
        /// </summary>
        /// <param name="topic">Name of topic.</param>
        /// <param name="user">Name of user.</param>
        /// <returns>A list with up to 100 messages for a topic and user.</returns>
        public IEnumerable<SaveMqttMessageBindingModel> Get(string topic, string user)
        {
            using var dbContext = new MqttDbContext();
            var messagesToReturn = dbContext.MqttMessages
                .Take(TopCount)
                .Where(m => m.Topic == topic && m.UserFrom == user)
                .OrderByDescending(m => m.Id)
                .ToList();

            return messagesToReturn;
        }

        // GET api/mqttMessage/5
        /// <summary>
        /// Returns the saves message with the given id.
        /// </summary>
        /// <param name="id">Id of message.</param>
        /// <returns>A single message with the given id.</returns>
        public SaveMqttMessageBindingModel Get(int id)
        {
            using var dbContext = new MqttDbContext();
            var messageToReturn = dbContext.MqttMessages.FirstOrDefault(m => m.Id == id);

            return messageToReturn;
        }

        // POST api/mqttMessage
        /// <summary>
        /// Saves a new message. Id is autoincrement.
        /// </summary>
        /// <param name="mqttMessage">Message Json object.</param>
        public void Post([FromBody]SaveMqttMessageBindingModel mqttMessage)
        {
            using var dbContext = new MqttDbContext();
            dbContext.MqttMessages.Add(mqttMessage);
            dbContext.SaveChanges();
        }

        // DELETE api/mqttMessage/5
        /// <summary>
        /// Deletes the message with the given id.
        /// </summary>
        /// <param name="id">Id of message.</param>
        public void Delete(int id)
        {
            using var dbContext = new MqttDbContext();
            var messageToDelete = dbContext.MqttMessages.FirstOrDefault(m => m.Id == id);

            if (messageToDelete != null)
                Delete(messageToDelete);
        }

        // DELETE api/mqttMessage/Message
        /// <summary>
        /// Deletes the given message.
        /// </summary>
        /// <param name="mqttMessage">Message Json object.</param>
        public void Delete([FromBody]SaveMqttMessageBindingModel mqttMessage)
        {
            using var dbContext = new MqttDbContext();
            dbContext.MqttMessages.Remove(mqttMessage);
            dbContext.SaveChanges();
        }
    }
}