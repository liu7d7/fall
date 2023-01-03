#version 450

layout(location = 2) uniform vec2 _screenSize;
layout(location = 3) uniform vec2 _radius;

in vec4 g_col;
in noperspective float g_u;
in noperspective float g_v;
in noperspective float g_line_width;
in noperspective float g_line_length;
in vec2 g_compare;
in vec2 g_pos;

out vec4 frag_color;

void main()
{
    if (g_compare.x == -1 && g_compare.y == -1)
    {
        /* We render a quad that is fattened by r, giving total width of the line to be w+r. We want smoothing to happen
           around w, so that the edge is properly smoothed out. As such, in the smoothstep function we have:
           Far edge   : 1.0                                          = (w+r) / (w+r)
           Close edge : 1.0 - (2r / (w+r)) = (w+r)/(w+r) - 2r/(w+r)) = (w-r) / (w+r)
           This way the smoothing is centered around 'w'.
         */
        float au = 1.0 - smoothstep(1.0 - ((2.0*_radius.x) / g_line_width), 1.0, abs(g_u / g_line_width));
        frag_color = g_col;
        frag_color.a = au;
        return;
    }
    float dist = distance(g_pos * _screenSize, g_compare * _screenSize);
    if (dist <= 6) {
        frag_color = g_col;
        frag_color.a = 1.0 - smoothstep(1.1667, 1.75, dist);
        return;
    }
    discard;
}