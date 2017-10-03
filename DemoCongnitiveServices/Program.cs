using Microsoft.ProjectOxford.Face.Contract;
using ServiceHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DemoCongnitiveServices
{
    internal class Program
    {
        private static void Main(string[] args)
        {

            //GetCleberPhotosByEuCorro();
            Console.Read();
        }


        private async static void GetCleberPhotosByEuCorro()
        {
            var faceService = new FaceDetectService();
            string personGroupId = "corridacleber161334";

            FaceServiceHelper.ApiKey = "";
            FaceServiceHelper.ApiKeyRegion = "";

            //var ExistsPersonGroup = FaceServiceHelper.GetPersonGroupsAsync(personGroupId).Result.PersonGroupId == personGroupId;

            //if (!ExistsPersonGroup)
            //{

            //}

            await FaceServiceHelper.CreatePersonGroupAsync(personGroupId, "Night Running");

            CreatePersonResult cleber = await FaceServiceHelper.CreatePersonAsync(personGroupId, "Cleber");

            const string friend1ImageDir = @"C:\Users\clebe\Desktop\templatescleber";

            foreach (string imagePath in Directory.GetFiles(friend1ImageDir, "*.jpg"))
            {
                using (Stream s = File.OpenRead(imagePath))
                {
                    try
                    {
                        await FaceServiceHelper.AddPersonFaceAsync(personGroupId, cleber.PersonId, s);
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
            }

            await FaceServiceHelper.TrainPersonGroupAsync(personGroupId);

            TrainingStatus trainingStatus = null;
            while (true)
            {
                trainingStatus = await FaceServiceHelper.GetPersonGroupTrainingStatusAsync(personGroupId);

                if (trainingStatus.Status != Status.Running)
                {
                    break;
                }

                await Task.Delay(1000);
            }

            //var countStart = 430067;
            var countStart = 432339;
            var countEnd = 432484;

            var targetPath = $@"{friend1ImageDir}\result";
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            for (int i = countStart; i < countEnd; i++)
            {
                string url = $"http://www.eucorro.com/fotos/evento_427/maior/foto_{i}.jpg";

                var image = await faceService.DetectFacesByUrl(url);

                var faceIds = image.Faces.Select(face => face.FaceId).ToArray();

                try
                {
                    var results = await FaceServiceHelper.IdentifyAsync(personGroupId, faceIds);

                    foreach (var identifyResult in results)
                    {
                        Console.WriteLine("Value i: {0}", i);
                        Console.WriteLine("");
                        Console.WriteLine("Result of face: {0}", identifyResult.FaceId);

                        if (identifyResult.Candidates.Length == 0)
                        {
                            Console.WriteLine("No one identified");
                        }
                        else
                        {
                            faceService.SaveImageByUrl(image, targetPath);
                        }
                    }
                }
                catch (Exception)
                {

                    continue;
                }

            }

        }

        private static void SimpleFaceDetectByDownload()
        {
            var faceService = new FaceDetectService();
            var faces = faceService.DetectFaceByUrl("http://www.eucorro.com/fotos/evento_427/maior/foto_430078.jpg");
        }

        private static void SimpleFaceDetect()
        {
            var faceService = new FaceDetectService();

            var template = faceService.SimpleDetectFace(@"C:\Users\clebe\Desktop\template.jpg").Result.First();
            var faces = faceService.SimpleDetectFace(@"C:\Users\clebe\Desktop\foto1.jpg").Result;
        }

        private static void FindByGroupPerson()
        {
            var faceService = new FaceDetectService();

            faceService.FindByGroupPerson();
        }

        private static void FindSimilar()
        {
            var faceService = new FaceDetectService();

            faceService.FindSimilar();
        }

        private static void FindSimilarGroups()
        {
            var faceService = new FaceDetectService();

            faceService.FindSimilarGroups();
        }
    }
}