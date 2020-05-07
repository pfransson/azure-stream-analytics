//********************************************************* 
// 
//    Copyright (c) Microsoft. All rights reserved. 
//    This code is licensed under the Microsoft Public License. 
//    THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF 
//    ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY 
//    IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR 
//    PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT. 
// 
//*********************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using TwitterClient.Common;
using Confluent.Kafka;
using System.Net;

namespace TwitterClient
{
    class Program
    {
        static void Main(string[] args)
        {
            //Configure Twitter OAuth
            
            var oauthToken = ConfigurationManager.AppSettings["oauth_token"];
            var oauthTokenSecret = ConfigurationManager.AppSettings["oauth_token_secret"];
            var oauthCustomerKey = ConfigurationManager.AppSettings["oauth_consumer_key"];
            var oauthConsumerSecret = ConfigurationManager.AppSettings["oauth_consumer_secret"];
			var searchGroups = ConfigurationManager.AppSettings["twitter_keywords"]; 
			var removeAllUndefined =  !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["clear_all_with_undefined_sentiment"]) ?
				Convert.ToBoolean(ConfigurationManager.AppSettings["clear_all_with_undefined_sentiment"])
				: false;
			var sendExtendedInformation = !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["send_extended_information"]) ?
			Convert.ToBoolean(ConfigurationManager.AppSettings["send_extended_information"])
			: false;
			var AzureOn = !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["AzureOn"]) ?
				Convert.ToBoolean(ConfigurationManager.AppSettings["AzureOn"])
				: false;
			var mode = ConfigurationManager.AppSettings["match_mode"]; 
			//Configure EventHub
			var config = new EventHubConfig();
            config.ConnectionString = ConfigurationManager.AppSettings["EventHubConnectionString"];
            config.EventHubName = ConfigurationManager.AppSettings["EventHubName"];

            //var myTargetObserver = new EventHubObserver(config, AzureOn);
            var kafkaConfig = new ProducerConfig
            {
                BootstrapServers = "pfwstreaming.westeurope.cloudapp.azure.com:9092",
                //BootstrapServers = "13.81.68.193:9092",
                ClientId = Dns.GetHostName()
            };
            //kafkaConfig.SaslUsername = "pfransson";
            //kafkaConfig.SaslPassword = "B3rgmanAlfa33";
            kafkaConfig.SecurityProtocol = SecurityProtocol.Plaintext;
            //kafkaConfig.SaslMechanism = SaslMechanism.Plain;
            //kafkaConfig.
            //kafkaConfig.
            //kafkaConfig.SaslMechanism = SaslMechanism.Plain;

            var myTargetObserver = new KafkaObserver(kafkaConfig, AzureOn);

            var keywords = searchGroups.Contains('|') ? string.Join(",", searchGroups.Split('|')) : searchGroups;
			var tweet = new Tweet();
				var datum = tweet.StreamStatuses(new TwitterConfig(oauthToken, oauthTokenSecret, oauthCustomerKey, oauthConsumerSecret,
				keywords, searchGroups)).Where(e => !string.IsNullOrWhiteSpace(e.Text)).Select(t => Sentiment.ComputeScore(t, searchGroups, mode)).Select(t => new Payload { ID = t.ID, CreatedAt = t.CreatedAt, Topic = t.Topic, SentimentScore = t.SentimentScore, Author = t.UserName, Text = t.Text, SendExtended = sendExtendedInformation, Language = t.Language,  TimeZone = t.TimeZone });
				if (removeAllUndefined)
				{
					datum = datum.Where(e => e.SentimentScore > -1);
				}
				datum.Where(e => e.Topic != "No Match").ToObservable().Subscribe(myTargetObserver);
        }
    }
}
