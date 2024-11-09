using System.Text.Json;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;


namespace appone
{
    class Program
    {
        static HttpClient client = new HttpClient();

        public static string detectFaceJson(string image_path)
        {
            MultipartFormDataContent request = new MultipartFormDataContent();
            FileStream image_data = File.OpenRead(image_path);
            request.Add(new StreamContent(image_data), "image", Path.GetFileName(image_path));
            request.Add(new StringContent("Mysecretkey"), "api_key");
            Task<HttpResponseMessage> outputTask = client.PostAsync("http://localhost:32168/v1/vision/face", request);
            outputTask.Wait();
            HttpResponseMessage output = outputTask.Result;
            Task<string> jsonStringTask = output.Content.ReadAsStringAsync();
            jsonStringTask.Wait();
            string jsonString = jsonStringTask.Result;

            return jsonString;
        }

        // public static async Task detectObjects(string image_path)
        // {

        //     var request = new MultipartFormDataContent();
        //     var image_data = File.OpenRead(image_path);
        //     request.Add(new StreamContent(image_data), "image", Path.GetFileName(image_path));
        //     request.Add(new StringContent("Mysecretkey"), "api_key");
        //     var output = await client.PostAsync("http://localhost:32168/v1/vision/detection", request);
        //     var jsonString = await output.Content.ReadAsStringAsync();
        //     Response response = JsonSerializer.Deserialize<Response>(jsonString);

        //     Console.WriteLine();
        //     Console.WriteLine($"{image_path} : {jsonString}");
        //     Console.WriteLine();

        //     System.IO.Directory.CreateDirectory("output/detection");

        //     using (var img = Image.Load(image_path))
        //     {
        //         foreach (DectectedObject obj in response.predictions)
        //         {
        //             img.Mutate(ctx =>
        //                      {
        //                          ctx.Draw(Color.Red, 2.0f, new RectangleF(obj.x_min, obj.y_min, obj.x_max - obj.x_min, obj.y_max - obj.y_min));
        //                          ctx.Fill(Color.Red, new RectangleF(obj.x_min, obj.y_min - 20, obj.x_max - obj.x_min, 20));
        //                          ctx.DrawText(obj.label, SystemFonts.CreateFont("Arial", 15), Color.White, new PointF(obj.x_min, obj.y_min - 20));
        //                      }

        //             );
        //         }
        //         img.Save($"output/detection/{image_path}");
        //     }
        // }

        // public static async Task detectScene(string image_path)
        // {

        //     var request = new MultipartFormDataContent();
        //     var image_data = File.OpenRead(image_path);
        //     request.Add(new StreamContent(image_data), "image", Path.GetFileName(image_path));
        //     request.Add(new StringContent("Mysecretkey"), "api_key");
        //     var output = await client.PostAsync("http://localhost:32168/v1/vision/scene", request);
        //     var jsonString = await output.Content.ReadAsStringAsync();

        //     Console.WriteLine();
        //     Console.WriteLine($"{image_path} : {jsonString}");
        //     Console.WriteLine();
        // }

        static void Main(string[] args)
        {

            // foreach (var file in Directory.GetFiles(".", "*.jpg"))
            // {
            //     Console.WriteLine($"Traitement de {file}:");
            //     detectFace(file).Wait();
            //     detectScene(file).Wait();
            //     detectObjects(file).Wait();
            // }

            // string jsonString = detectFaceJson("Photo le 2022-02-18 à 15.53.jpg");
            // Console.Out.WriteLine(jsonString);

            foreach (string image_path in args)
            {
                if (!File.Exists(image_path))
                {
                    Console.WriteLine($"Le fichier {image_path} n'existe pas.");
                }
                else
                {
                    string jsonString = detectFaceJson(image_path);
                    Console.Out.WriteLine(jsonString);
                    Response? response = JsonSerializer.Deserialize<Response>(jsonString);
                    if (response is not null)
                    {
                        DrawObjectBoundingBox(image_path, response.predictions, "Face");
                    }
                }
            }
        }

        private static void DrawObjectBoundingBox(string image_path, DectectedObject[] predictions, string default_label)
        {
            System.IO.Directory.CreateDirectory("output/detection");

            using (var img = Image.Load(image_path))
            {
                foreach (DectectedObject obj in predictions)
                {
                    img.Mutate(ctx =>
                             {
                                 ctx.Draw(Color.Red, 2.0f, new RectangleF(obj.x_min, obj.y_min, obj.x_max - obj.x_min, obj.y_max - obj.y_min));
                                 ctx.Fill(Color.Red, new RectangleF(obj.x_min, obj.y_min - 20, obj.x_max - obj.x_min, 20));
                                 ctx.DrawText(obj.label??default_label, SystemFonts.CreateFont("Arial", 15), Color.White, new PointF(obj.x_min, obj.y_min - 20));
                             }

                    );
                }
                img.Save($"output/detection/{image_path}");
            }
        }

    }

    class Response
    {

        public bool success { get; set; }
        public DectectedObject[] predictions { get; set; }

    }

    class DectectedObject
    {

        public string label { get; set; }
        public float confidence { get; set; }
        public int y_min { get; set; }
        public int x_min { get; set; }
        public int y_max { get; set; }
        public int x_max { get; set; }

    }
}