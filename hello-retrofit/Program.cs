﻿using System;
using WebApiClient;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using WebApiClient.Defaults;
using WebApiClient.Parameterables;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace hello_retrofit
{
    class Program
    {
        static void Main(string[] args)
        {
            //调用Values Service
            using (var client = HttpApiClient.Create<IValuesApiCaller>())
            {
                Console.WriteLine("-----Invoke Values Service-----");
                var results = client.GetValues().InvokeAsync().Result;
                Console.WriteLine($"results is {results}");
                var result = client.GetValue(10).InvokeAsync().Result;
                Console.WriteLine($"result is {result}");
            }

            //调用Student Service
            using (var client = HttpApiClient.Create<IStudentApiCaller>())
            {
                Console.WriteLine("-----Invoke Student Service-----");
                var students = client.GetAllStudents().InvokeAsync().Result;
                students.ToList().ForEach(student =>
                {
                    Console.WriteLine($"student: {student.Name}");
                });
                var stu = new Student() { Name = "Payne", Age = 26 };
                var result = client.NewStudent(stu).InvokeAsync().Result;
                Console.WriteLine($"result is {result}");
            }

            //调用Files Service
            using (var client = HttpApiClient.Create<IFilesApiCaller>())
            {
                Console.WriteLine("-----Invoke File Service-----");
                var files = new string[]
                {
                    @"C:\Users\PayneQin\Videos\Rec 0001.mp4",
                    @"C:\Users\PayneQin\Videos\Rec 0002.mp4",
                }
                .Select(f=>new MulitpartFile(f))
                .ToList();
                var result = client.Upload(files).InvokeAsync().Result;
                Console.WriteLine(result);

                var json = JArray.Parse(result);
                var fileId = ((JObject)json.First)["fileId"].Value<string>();
                var fileName = Path.Combine(Environment.CurrentDirectory, "Output/Video001.mp4");
                var filePath = Path.GetDirectoryName(fileName);
                if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
                using (var fileStram = new FileStream(fileName, FileMode.Create))
                {
                    var stream = client.Download(fileId).InvokeAsync().Result;
                    stream.Content.ReadAsStreamAsync().Result.CopyToAsync(fileStram);
                }
            }
        }

        static MultipartFormDataContent BuildContent(IEnumerable<string> files)
        {
            var boundary = "---------------" + DateTime.Now.Ticks.ToString("x");
            var content = new MultipartFormDataContent(boundary);
            files.ToList().ForEach(file =>
            {
                var fileName = Path.GetFileName(file);
                var fileStream = new FileStream(file, FileMode.Open);
                content.Add(new StreamContent(fileStream), "multipartFile", fileName);
            });

            return content;
        }
    }
}
