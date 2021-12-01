﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

using FinanceLib;

namespace Client
{
    class Program
    {
        static void Main()
        {
            string user, password;
            UserAccount userAccount;

            // Getting the data to check
            Console.WriteLine("Login to BANK NAME app");
            Console.Write("Insert the username: ");
            user = Console.ReadLine();
            Console.Write("Insert the password: ");
            password = Console.ReadLine();

            // Transforms the data to a json file
            var loginData = new {
                usr = user,
                pwd = password
            };
            string jsonString = JsonSerializer.Serialize(loginData);

            // Sends the data to the server for a check
            string jsonResp = GetUserAccount(jsonString);
            var response = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResp);

            // Manages the response
            if (response["valid"].ToString() == "true")
            {
                var data = (JsonElement)response["data"];
                userAccount = new UserAccount() {
                    Username = data.GetProperty("user").ToString(),
                    BankAccount = new BankAccount() {
                        IBAN = data.GetProperty("iban").ToString(),
                        Cash = data.GetProperty("cash")
                    }
                };
                Console.WriteLine("Your IBAN is: " + userAccount.BankAccount.IBAN);
                Console.WriteLine("Your cash is: " + userAccount.BankAccount.Cash);
            }
            else
                Console.WriteLine("Invalid login data!");
        }

        static string GetUserAccount(string jsonString)
        {
            // Input stream data buffer and bytes received
            byte[] inBuffer = new byte[1024];
            int inBytes = 0;

            try {
                // Sets up the remote end point to match the localhost on port 11000
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

                // Creates the TCP/IP socket
                Socket sender = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);
                
                try {
                    // Enstablishes the connection with the chosen end point
                    sender.Connect(remoteEP);
                    Console.WriteLine("Successfully connected to {0}", sender.RemoteEndPoint);

                    // Sets up the message and sends it to the remote device
                    byte[] outBuffer = System.Text.Encoding.ASCII.GetBytes(jsonString);
                    int outBytes = sender.Send(outBuffer);

                    // Receives the response from the remote device
                    inBytes = sender.Receive(inBuffer);
                    
                    // Releases the socket
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                }
                catch (ArgumentNullException e) {
                    Console.WriteLine("ArgumentNullException: " + e.ToString());
                }
                catch (SocketException e) {
                    Console.WriteLine("SocketException: " + e.ToString());
                }
                catch (Exception e) {
                    Console.WriteLine("Unexpected exception: " + e.ToString());
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

            // Converts the data to a string and returns it
            return System.Text.Encoding.ASCII.GetString(inBuffer, 0, inBytes);
        }
    }
}