using OpenTK.Graphics.OpenGL4;
using Fall.Shared;
using StbImageSharp;

namespace Fall.Engine
{
    public class skybox_texture
    {
        public readonly int[] ids;
        
        private static readonly Dictionary<string, TextureTarget> _targets = 
            maps.create(new KeyValuePair<string, TextureTarget>("pos_x", TextureTarget.TextureCubeMapPositiveX),
                new KeyValuePair<string, TextureTarget>("neg_x", TextureTarget.TextureCubeMapNegativeX),
                new KeyValuePair<string, TextureTarget>("pos_y", TextureTarget.TextureCubeMapPositiveY),
                new KeyValuePair<string, TextureTarget>("neg_y", TextureTarget.TextureCubeMapNegativeY),
                new KeyValuePair<string, TextureTarget>("pos_z", TextureTarget.TextureCubeMapPositiveZ),
                new KeyValuePair<string, TextureTarget>("neg_z", TextureTarget.TextureCubeMapNegativeZ));

        public static skybox_texture load_from_file(string path)
        {
            int idx = 0;
            int[] ids = new int[6];
            foreach ((string dir, TextureTarget _) in _targets)
            {
                using Stream stream = File.OpenRead($"{path}_{dir}.png");
                ImageResult img = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                ids[idx] = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, ids[idx]);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, img.Width, img.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, img.Data);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (int) TextureWrapMode.ClampToEdge);
                idx++;
            }

            return new skybox_texture(ids);
        }
        
        private skybox_texture(int[] id)
        {
            ids = id;
        }
    }
}