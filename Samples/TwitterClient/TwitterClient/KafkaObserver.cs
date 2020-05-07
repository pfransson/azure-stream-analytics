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
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
//using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System.Configuration;
using Confluent.Kafka;
using TwitterClient.Common;

namespace TwitterClient
{
    public class KafkaObserver : IObserver<Payload>
    {
        private ProducerConfig _config;
        private IProducer<Null, string> _kafkaClient;
        public bool AzureOn { get; set; }
                
        public KafkaObserver(ProducerConfig config, bool azureOn = true)
        {
			AzureOn = azureOn;
            try
            {
				
                _config = config;
				if (AzureOn)
				{
                    _kafkaClient = new ProducerBuilder<Null, string>(config).Build();
				}
            }
            catch (Exception ex)
            {
               
            }

        }

        public void handle(DeliveryReport<Null,string> deliveryReport)
        {
            Console.WriteLine("Message delivered, id: " + deliveryReport.Key);
        }
        public void OnNext(Payload TwitterPayloadData)
        {
            try
            {
				var serialisedString = JsonConvert.SerializeObject(TwitterPayloadData);
				if (AzureOn)
				{
                    _kafkaClient.Produce("twittercorona",new Message<Null, string>{ Value = serialisedString }, handle);                   
                    Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine("Sending" + serialisedString + " at: " + TwitterPayloadData.CreatedAt.ToString());
				}
				else
				{
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine("Faked Sending" + serialisedString + " at: " + TwitterPayloadData.CreatedAt.ToString());
				}
          
                                
            }
            catch (Exception ex)
            {
                
            }

        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            
        }

    }
}
