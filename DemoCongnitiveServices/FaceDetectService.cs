using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using ServiceHelpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DemoCongnitiveServices
{
    public class FaceDetectService
    {
        public FaceDetectService()
        {
            FaceServiceHelper.ApiKey = "";
            FaceServiceHelper.ApiKeyRegion = "";
        }

        public async Task<Face[]> DetectFaceByUrl(string url)
        {
            IEnumerable<FaceAttributeType> faceAttributes =
                new FaceAttributeType[] { FaceAttributeType.Gender, FaceAttributeType.Age, FaceAttributeType.Smile, FaceAttributeType.Emotion, FaceAttributeType.Glasses, FaceAttributeType.Hair };

            try
            {
                Face[] faces = await FaceServiceHelper.DetectAsync(url, returnFaceId: true, returnFaceLandmarks: false, returnFaceAttributes: faceAttributes);
                return faces;
            }
            catch (FaceAPIException f)
            {
                return new Face[0];
            }
            catch (Exception e)
            {
                return new Face[0];
            }
        }

        public async Task<Face[]> SimpleDetectFace(string imageFilePath)
        {
            // The list of Face attributes to return.
            IEnumerable<FaceAttributeType> faceAttributes =
                new FaceAttributeType[] { FaceAttributeType.Gender, FaceAttributeType.Age, FaceAttributeType.Smile, FaceAttributeType.Emotion, FaceAttributeType.Glasses, FaceAttributeType.Hair };

            // Call the Face API.
            try
            {
                using (Stream imageFileStream = File.OpenRead(imageFilePath))
                {
                    Face[] faces = await FaceServiceHelper.DetectAsync(imageFileStream, returnFaceId: true, returnFaceLandmarks: false, returnFaceAttributes: faceAttributes);
                    return faces;
                }
            }
            // Catch and display Face API errors.
            catch (FaceAPIException f)
            {
                return new Face[0];
            }
            // Catch and display all other errors.
            catch (Exception e)
            {
                return new Face[0];
            }
        }

        public async void FindByGroupPerson()
        {
            string personGroupId = "formatura";
            var ExistsPersonGroup = FaceServiceHelper.GetPersonGroupsAsync(personGroupId).Result.PersonGroupId == personGroupId;

            if (!ExistsPersonGroup)
            {
                await FaceServiceHelper.CreatePersonGroupAsync(personGroupId, "Minha formatura");
            }

            CreatePersonResult ana = await FaceServiceHelper.CreatePersonAsync(personGroupId, "Ana");

            const string friend1ImageDir = @"M:\Onedrive\Projetos\Detector Face beta\ana";

            foreach (string imagePath in Directory.GetFiles(friend1ImageDir, "*.jpg"))
            {
                using (Stream s = File.OpenRead(imagePath))
                {
                    try
                    {
                        await FaceServiceHelper.AddPersonFaceAsync(personGroupId, ana.PersonId, s);
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

            string template = @"C:\Users\clebe\Desktop\template2.jpg";

            using (Stream s = File.OpenRead(template))
            {
                var faces = await FaceServiceHelper.DetectAsync(s);
                var faceIds = faces.Select(face => face.FaceId).ToArray();

                var results = await FaceServiceHelper.IdentifyAsync(personGroupId, faceIds);
                foreach (var identifyResult in results)
                {
                    Console.WriteLine("Result of face: {0}", identifyResult.FaceId);
                    if (identifyResult.Candidates.Length == 0)
                    {
                        Console.WriteLine("No one identified");
                    }
                    else
                    {
                        var candidateId = identifyResult.Candidates[0].PersonId;
                        var person = await FaceServiceHelper.GetPersonAsync(personGroupId, candidateId);
                        Console.WriteLine("Identified as {0}", person.Name);
                    }
                }

                Console.ReadKey();
            }
        }

        public async void FindSimilar()
        {
            var images = new List<Image>();

            const string imagemDir = @"M:\Onedrive\Projetos\Detector Face beta\Moda CESUMAR";

            foreach (string imagePath in Directory.GetFiles(imagemDir, "*.jpg"))
            {
                try
                {
                    Console.WriteLine($"image: {imagePath}");
                    var image = await DetectFaces(imagePath);

                    images.Add(image);
                }
                catch (Exception ex)
                {
                    continue;
                }
            }

            var template = SetupTemplate(@"C:\Users\clebe\Desktop\template.jpg").Result;

            var result = FindSimilar(template, images);
        }

        public async void FindSimilarGroups()
        {
            var images = new List<Image>();

            const string imagemDir = @"M:\Onedrive\Projetos\Detector Face beta\Moda CESUMAR";

            foreach (string imagePath in Directory.GetFiles(imagemDir, "*.jpg"))
            {
                try
                {
                    Console.WriteLine($"image: {imagePath}");
                    var image = await DetectFaces(imagePath);

                    images.Add(image);
                }
                catch (Exception ex)
                {
                    continue;
                }
            }

            var faceIds = images.SelectMany(x => x.Faces).Select(t => t.FaceId).ToArray();

            GroupResult group = await FaceServiceHelper.GroupAsync(faceIds);

            CreateDirectoryByGroupResult(imagemDir, group, images);
        }

        private void CreateDirectoryByGroupResult(string path, GroupResult group, IList<Image> images)
        {
            path = $@"{path}\result";
            var groupPath = string.Empty;
            IList<Image> imagesSelected = new List<Image>();

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var i = 0;
            group.Groups.ForEach(item =>
            {
                var groupName = "group" + i;
                groupPath = $@"{path}\{groupName}";

                if (!Directory.Exists(groupPath))
                {
                    Directory.CreateDirectory(groupPath);
                }

                imagesSelected = SelectRecognizedImages(images, item);
                SaveAllImages(imagesSelected, groupPath);

                i++;
            });


            groupPath = $@"{path}\unknow";
            if (!Directory.Exists(groupPath))
            {
                Directory.CreateDirectory(groupPath);
            }

            imagesSelected = SelectRecognizedImages(images, group.MessyGroup);
            SaveAllImages(imagesSelected, groupPath);

        }

        private void SaveAllImages(IList<Image> images, string targetPath)
        {
            string destFile = string.Empty;
            var i = 0;
            foreach (var image in images)
            {
                destFile = Path.Combine(targetPath, GenerateString(20));
                //destFile = destFile + "." + image.Path.Split('.')[1];
                destFile = destFile + "_" + i + ".JPG";
                //if (File.Exists(destFile))
                //{
                //    File.Delete(destFile);
                //}

                File.Copy(image.Path, destFile);
                i++;
            }
        }

        public void SaveImageByUrl(Image image, string targetPath)
        {
            string destFile = Path.Combine(targetPath, GenerateString(20));
            destFile = destFile + "." + image.Path.Split('.').Last();

            if (File.Exists(destFile))
            {
                File.Delete(destFile);
            }

            var img = Bitmap.FromStream(new MemoryStream(new WebClient().DownloadData(image.Path)));

            img.Save(destFile);
        }

        public string GenerateString(int size)
        {
            Random rand = new Random();
            string Alphabet = "abcdefghijklmnopqrstuvwyxzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            char[] chars = new char[size];
            for (int i = 0; i < size; i++)
            {
                chars[i] = Alphabet[rand.Next(Alphabet.Length)];
            }
            return new string(chars);
        }

        /// <summary>
        /// Detects the faces.
        /// </summary>
        /// <param name="imagePath">The image path.</param>
        /// <returns></returns>
        public async Task<Image> DetectFaces(string imagePath)
        {
            var image = new Image() { Id = new Guid(), Path = imagePath };

            using (Stream s = File.OpenRead(imagePath))
            {
                var faces = await FaceServiceHelper.DetectAsync(s);
                image.Faces = faces;
            }

            return image;
        }

        /// <summary>
        /// Detects the faces.
        /// </summary>
        /// <param name="imagePath">The image path.</param>
        /// <returns></returns>
        public async Task<Image> DetectFacesByUrl(string url)
        {
            var image = new Image() { Id = new Guid(), Path = url };

            var faces = await FaceServiceHelper.DetectAsync(url);
            image.Faces = faces;

            return image;
        }


        private async Task<Guid> SetupTemplate(string imagePath)
        {
            using (Stream s = File.OpenRead(imagePath))
            {
                var faces = await FaceServiceHelper.DetectAsync(s);
                var faceIds = faces.Select(face => face.FaceId).ToArray();

                return faceIds.First();
            }
        }

        /// <summary>
        /// Finds the similar face.
        /// </summary>
        /// <param name="facePatternId">The face pattern identifier.</param>
        /// <param name="images">The images.</param>
        /// <returns></returns>
        public async Task<IList<Image>> FindSimilar(Guid facePatternId, IList<Image> images)
        {
            var responseContent = await FaceServiceHelper.FindSimilarAsync(facePatternId, images.SelectMany(e => e.Faces).Select(e => e.FaceId).ToArray());
            return SelectRecognizedImages(images, responseContent.Select(e => e.FaceId).ToList());
        }

        /// <summary>
        /// Selects the recognized images.
        /// </summary>
        /// <param name="imagesCollection">The images collection.</param>
        /// <param name="recognizedFacesIds">The recognized faces ids.</param>
        /// <returns></returns>
        private IList<Image> SelectRecognizedImages(IList<Image> imagesCollection, IList<Guid> recognizedFacesIds)
        {
            var imagesSource = imagesCollection;
            var result = new List<Image>();

            result = imagesCollection.Where(x => x.Faces.Any(t => recognizedFacesIds.Contains(t.FaceId))).ToList();

            Console.Clear();
            Console.WriteLine("Resultado:");
            Console.WriteLine("");
            foreach (var item in result)
            {
                Console.WriteLine($"faceId: {item.Path}");
            }

            return result;
        }
    }
}