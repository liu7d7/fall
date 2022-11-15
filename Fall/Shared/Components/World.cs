using Fall.Engine;
using OpenTK.Mathematics;

namespace Fall.Shared.Components
{
    public class world
    {
        private readonly Dictionary<Vector2i, chunk> _chunks;
        
        public world()
        {
            _chunks = new Dictionary<Vector2i, chunk>();
        }

        private Vector2i _prevCPos;

        public void update()
        {
            Vector2i chunkPos = fall.player.pos.Xz.to_chunk_pos();
            if (_prevCPos == chunkPos)
            {
                return;
            }
            for (int i = -12; i <= 12; i++)
            {
                for (int j = -12; j <= 12; j++)
                {
                    int i1 = i + chunkPos.X;
                    int j1 = j + chunkPos.Y;
                    if (!_chunks.ContainsKey((i1, j1)))
                    {
                        _chunks[(i1, j1)] = new chunk((i1, j1));
                    }
                }
            }
            _prevCPos = chunkPos;
        }
        
        public void render()
        {
            Vector2i chunkPos = fall.player.pos.Xz.to_chunk_pos();
            for (int i = -8; i <= 8; i++)
            {
                for (int j = -8; j <= 8; j++)
                {
                    _chunks[(i + chunkPos.X, j + chunkPos.Y)].mesh.render();
                }
            }
        }

        public static float height_at(Vector2 vec)
        {
            return chunk.height_at(vec);
        }
    }

    public struct chunk
    {
        public readonly mesh mesh;
        private const float div = 30f;
        private const int wh = 16;

        public chunk(Vector2i chunkPos)
        {
            mesh = new mesh(mesh.draw_mode.triangle, render_system.basic, true, vao.attrib.float3);

            mesh.begin();
            for (int i = 0; i < wh; i++)
            {
                for (int j = 0; j < wh; j++)
                {
                    mesh.quad(
                        mesh.float3(i + chunkPos.X * 16, noise(i + chunkPos.X * 16, j + 1 + chunkPos.Y * 16), j + chunkPos.Y * 16 + 1).next(),
                        mesh.float3(i + chunkPos.X * 16 + 1, noise(i + 1 + chunkPos.X * 16, j + 1 + chunkPos.Y * 16), j + chunkPos.Y * 16 + 1).next(),
                        mesh.float3(i + chunkPos.X * 16 + 1, noise(i + 1 + chunkPos.X * 16, j + chunkPos.Y * 16), j + chunkPos.Y * 16).next(),
                        mesh.float3(i + chunkPos.X * 16, noise(i + chunkPos.X * 16, j + chunkPos.Y * 16), j + chunkPos.Y * 16).next()
                    );
                }
            }
            mesh.end();
        }

        private static float noise(int x, int y)
        {
            return SimplexNoise.Noise.CalcPixel2D(x, y, 0.01f) / div;
        }

        public static float height_at(Vector2 vec)
        {
            int x1 = (int)MathF.Floor(vec.X);
            int y1 = (int)MathF.Floor(vec.Y);
            
            float v00 = noise(x1, y1);
            float v10 = noise(x1 + 1, y1);
            float v01 = noise(x1, y1 + 1);
            float v11 = noise(x1 + 1, y1 + 1);
            float x = vec.X - x1;
            float y = vec.Y - y1;
            
            // bilinear interpolation
            return (1 - x) * (1 - y) * v00 + x * (1 - y) * v10 + (1 - x) * y * v01 + x * y * v11;
        }
    }
}