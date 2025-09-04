using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace VectorModelTest.Services
{
    public class EmbeddingService
    {
        public EmbeddingService() { }

        public float[] CreateImageEmbedding(Image<Rgb24> image, string model)
        {
            switch (model)
            {
                case "resnet50-v1-12-int8.onnx":
                    return CreateImageEmbeddingResnet(image, model);
                case "resnet50-v2-7.onnx":
                    return CreateImageEmbeddingResnet(image, model);
                case "resnet152-v2-7.onnx":
                    return CreateImageEmbeddingResnet(image, model);
                case "vgg19-7.onnx":
                    return CreateImageEmbeddingResnet(image, model);
                case "efficientnet-lite4-11.onnx":
                    return CreateImageEmbeddingEfficientNet(image, model);
                default:
                    return new float[0];

            }
        }

        public float[] CreateImageEmbeddingResnet(Image<Rgb24> image, string model)
        {

           
            string modelFilePath ="Onnx/"+model;

            using Stream imageStream = new MemoryStream();
            image.Mutate(x =>
            {
                x.Resize(new ResizeOptions
                {
                    Size = new Size(224, 224),
                    Mode = ResizeMode.Crop
                });
            });
            image.Save(imageStream, image.Metadata.DecodedImageFormat);

            Tensor<float> input = new DenseTensor<float>(new[] { 1, 3, 224, 224 });
            var mean = new[] { 0.485f, 0.456f, 0.406f };
            var stddev = new[] { 0.229f, 0.224f, 0.225f };
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    Span<Rgb24> pixelSpan = accessor.GetRowSpan(y);
                    for (int x = 0; x < accessor.Width; x++)
                    {
                        input[0, 0, y, x] = ((pixelSpan[x].R / 255f) - mean[0]) / stddev[0];
                        input[0, 1, y, x] = ((pixelSpan[x].G / 255f) - mean[1]) / stddev[1];
                        input[0, 2, y, x] = ((pixelSpan[x].B / 255f) - mean[2]) / stddev[2];
                    }
                }
            });

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("data", input)
            };

            using var session = new InferenceSession(modelFilePath);
            using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);

            IEnumerable<float> output = results.First().AsEnumerable<float>();
            float[] vector = output.ToArray();



            return vector;
        }


        public float[] CreateImageEmbeddingEfficientNet(Image<Rgb24> image, string model)
        {
            string modelFilePath = "Onnx/" + model;

            using Stream imageStream = new MemoryStream();
            image.Mutate(x =>
            {
                x.Resize(new ResizeOptions
                {
                    Size = new Size(224, 224),
                    Mode = ResizeMode.Crop
                });
            });
            image.Save(imageStream, image.Metadata.DecodedImageFormat);

            Tensor<float> input = new DenseTensor<float>(new[] { 1, 224, 224, 3 });
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    Span<Rgb24> pixelSpan = accessor.GetRowSpan(y);
                    for (int x = 0; x < accessor.Width; x++)
                    {
                        input[0, 0, y, x] = (pixelSpan[x].R - 127) / 128f;
                        input[0, 1, y, x] = (pixelSpan[x].G - 127) / 128f;
                        input[0, 2, y, x] = (pixelSpan[x].B - 127) / 128f;
                    }
                }
            });

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("images:0", input)
            };

            using var session = new InferenceSession(modelFilePath);
            using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);

            IEnumerable<float> output = results.First().AsEnumerable<float>();
            float[] vector = output.ToArray();



            return vector;
        }

    }
}
