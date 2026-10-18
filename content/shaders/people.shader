vertex {
  //
  // The park's people, and the heads, thought bubbles and balloons drawn with them: camera-facing
  // quads built in world space, each carrying its own patch of one atlas and its own colour. They
  // write depth, as the original's do, so the fragment shader drops what its alpha test drops.
  //
  // There is no colour in the vertex format, so the colour rides in the material flags word as
  // 0xAARRGGBB, the same arrangement content/shaders/particles.shader uses and for the same
  // reason. The model matrix is identity: the quads are already in world space, turned to face
  // the camera as they were written.
  //
  layout( location = 0 ) in vec3 position;
  layout( location = 1 ) in vec3 normal;
  layout( location = 2 ) in vec2 texCoords;
  layout( location = 3 ) in int texIndex;
  layout( location = 4 ) in uint matFlags;

  layout( set = 0, binding = 0 ) uniform ObjectUniformBuffer {
      mat4 g_mModel;
      mat4 g_mView;
      mat4 g_mProj;
  } g_oUbo;

  layout( location = 0 ) out struct VS_OUT {
      vec2 vTexCoords;
      vec4 vColour;
  } vs_out;

  void main() {
      vs_out.vTexCoords = texCoords;
      vs_out.vColour = vec4(
          float( ( matFlags >> 16 ) & 0xFFu ),
          float( ( matFlags >> 8 ) & 0xFFu ),
          float( matFlags & 0xFFu ),
          float( ( matFlags >> 24 ) & 0xFFu ) ) / 255.0;

      gl_Position = g_oUbo.g_mProj * g_oUbo.g_mView * g_oUbo.g_mModel * vec4( position, 1.0 );
  }
}

fragment {
  layout( location = 0 ) in struct VS_OUT {
      vec2 vTexCoords;
      vec4 vColour;
  } vs_out;

  layout( location = 0 ) out vec4 fragColor;

  layout( set = 0, binding = 0 ) uniform ObjectUniformBuffer {
      mat4 g_mModel;
      mat4 g_mView;
      mat4 g_mProj;
  } g_oUbo;

  layout( set = 1, binding = 0 ) uniform texture2D Color;
  layout( set = 1, binding = 1 ) uniform sampler s_Color;

  void main()
  {
      fragColor = texture( sampler2D( Color, s_Color ), vs_out.vTexCoords ) * vs_out.vColour;

      // The original's alpha test for a graded picture (state word 0x556): ALPHAFUNC GREATEREQUAL
      // against ALPHAREF 16 (docs/exe/render-states.md). What it drops writes no depth, so the clear
      // pixels round a picture hide nothing behind it.
      if ( fragColor.a < 16.0 / 255.0 )
          discard;
  }
}
