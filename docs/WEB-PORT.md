# Browser port assessment

Status: source assessment, not a working browser game.

## Present architecture

- source/OpenTPW/Client/Renderer.cs creates a native Vulkan device and swapchain through NeoVeldrid.
- OpenTPW.Common window, display and input use native SDL2 bindings.
- source/OpenTPW/Audio/Audio.cs opens a native SDL audio device.
- GameDir, BaseFileSystem and save code use normal disk folders and System.IO.
- UI shader startup uses NeoVeldrid.SPIRV; content assets include Vulkan GLSL.

Compiling the current executable to WebAssembly alone does not provide these native services in a browser.

## Recommended gates

1. Keep this desktop fork working with Polish data and keep decoding tests independent of the renderer.
2. Browser proof: load user-selected language files locally, decode the same BFMU/BFST corpus, display Polish text and a bitmap-font sample. No original assets are hosted.
3. Renderer proof: render one original static mesh and texture with a browser graphics backend before porting a whole park. Decide between a compatible .NET/browser graphics backend and a separate WebGPU/WebGL presentation layer only after that test.
4. Add browser input, audio, user-provided asset access, and durable saves (with explicit import/export).
5. Enter the lobby, then one Lost Kingdom park; measure frame time, memory and load time before wider gameplay work.

The C++ tpw-port is a separate candidate, but it currently targets macOS and states that no license has been chosen. Its documented build also needs specific Mac originals. Do not copy its code into this MIT fork without resolving permission and compatibility.

## Delivery boundary

No browser game, WebAssembly build, browser renderer or hosting deployment has been produced by this change. This document records concrete platform dependencies and testable next steps rather than claiming that a web wrapper would make the native engine portable.
