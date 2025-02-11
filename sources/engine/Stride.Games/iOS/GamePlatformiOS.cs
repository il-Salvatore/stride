// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
#if STRIDE_PLATFORM_IOS
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Stride.Graphics;

namespace Stride.Games
{
    internal class GamePlatformiOS : GamePlatform, IGraphicsDeviceFactory
    {
        [DllImport("/usr/lib/libSystem.dylib")]
        private static unsafe extern int sysctlbyname([MarshalAs(UnmanagedType.LPStr)] string name, IntPtr oldp, int* oldlenp, IntPtr newp, uint newlen);

        private unsafe void PopulateFullName()
        {
            int len;
            sysctlbyname("hw.machine", IntPtr.Zero, &len, IntPtr.Zero, 0);
            if (len == 0) return;

            var output = Marshal.AllocHGlobal(len);
            sysctlbyname("hw.machine", output, &len, IntPtr.Zero, 0);
            FullName = Marshal.PtrToStringAnsi(output);
            Marshal.FreeHGlobal(output);
        }

        public GamePlatformiOS(GameBase game) : base(game)
        {
            PopulateFullName();
        }

        public override string DefaultAppDirectory
        {
            get
            {
                var assemblyUri = new Uri(Assembly.GetEntryAssembly().CodeBase);
                return Path.GetDirectoryName(assemblyUri.LocalPath);
            }
        }

        internal override GameWindow GetSupportedGameWindow(AppContextType type)
        {
            if (type == AppContextType.iOS)
            {
                return new GameWindowSDL();
            }
            else
            {
                return null;
            }
        }

        public override List<GraphicsDeviceInformation> FindBestDevices(GameGraphicsParameters preferredParameters)
        {
            var gameWindowiOS = gameWindow as GameWindowSDL;
            if (gameWindowiOS != null)
            {
                var graphicsAdapter = GraphicsAdapterFactory.Default;
                var graphicsDeviceInfos = new List<GraphicsDeviceInformation>();
                var preferredGraphicsProfiles = preferredParameters.PreferredGraphicsProfile;
                foreach (var featureLevel in preferredGraphicsProfiles)
                {
                    // Check if this profile is supported.
                    if (graphicsAdapter.IsProfileSupported(featureLevel))
                    {
                        // Everything is already created at this point, just transmit what has been done
                        var deviceInfo = new GraphicsDeviceInformation
                        {
                            Adapter = GraphicsAdapterFactory.Default,
                            GraphicsProfile = featureLevel,
                            PresentationParameters = new PresentationParameters(preferredParameters.PreferredBackBufferWidth,
                                                                                preferredParameters.PreferredBackBufferHeight,
                                                                                gameWindowiOS.NativeWindow)
                            {
                                // TODO: PDX-364: Transmit what was actually created
                                BackBufferFormat = preferredParameters.PreferredBackBufferFormat,
                                DepthStencilFormat = preferredParameters.PreferredDepthStencilFormat,
                            }
                        };

                        graphicsDeviceInfos.Add(deviceInfo);

                        // If the profile is supported, we are just using the first best one
                        break;
                    }
                }

                return graphicsDeviceInfos;
            }
            return base.FindBestDevices(preferredParameters);
        }

        public override void DeviceChanged(GraphicsDevice currentDevice, GraphicsDeviceInformation deviceInformation)
        {
            // TODO: Check when it needs to be disabled on iOS (OpenGL)?
            // Force to resize the gameWindow
            //gameWindow.Resize(deviceInformation.PresentationParameters.BackBufferWidth, deviceInformation.PresentationParameters.BackBufferHeight);
        }
    }
}
#endif
