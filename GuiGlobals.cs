using System;
using Terminal.Gui;
using TAttribute = Terminal.Gui.Attribute;

namespace Archiver
{
    public static class GuiGlobals
    {
        public static class Colors
        {
            private static bool _isInitalized = false;

            public static ColorScheme GlobalScheme;
            public static ColorScheme StatusBar;

            public static ColorScheme Green;
            public static ColorScheme Red;
            public static ColorScheme Blue;
            public static ColorScheme Yellow;

            public static void InitColorSchemes()
            {
                if (!_isInitalized)
                {
                    GlobalScheme = new ColorScheme()
                    {
                        Normal = new TAttribute(Color.Gray, Color.Black),
                        Focus = new TAttribute(Color.Gray, Color.DarkGray),
                        HotNormal = new TAttribute(Color.White, Color.Black),
                        HotFocus = new TAttribute(Color.Brown, Color.DarkGray),
                    };

                    StatusBar = new ColorScheme()
                    {
                        Normal = new TAttribute(Color.White, Color.Blue),
                        Focus = new TAttribute(Color.White, Color.Black),
                        HotNormal = new TAttribute(Color.Brown, Color.Blue),
                        HotFocus = new TAttribute(Color.BrightYellow, Color.Black),
                        Disabled = new TAttribute(Color.Gray, Color.DarkGray)
                    };

                    

                    Green = new ColorScheme()
                    {
                        Normal = new TAttribute(Color.Green, GlobalScheme.Normal.Background),
                        Focus = new TAttribute(Color.Green, GlobalScheme.Focus.Background),
                        Disabled = new TAttribute(Color.DarkGray, GlobalScheme.Normal.Background)
                    };

                    Red = new ColorScheme()
                    {
                        Normal = new TAttribute(Color.Red, GlobalScheme.Normal.Background),
                        Focus = new TAttribute(Color.Red, GlobalScheme.Focus.Background),

                    };

                    Blue = new ColorScheme()
                    {
                        Normal = new TAttribute(Color.Blue, GlobalScheme.Normal.Background),
                        Focus = new TAttribute(Color.Blue, GlobalScheme.Focus.Background),

                    };

                    Yellow = new ColorScheme()
                    {
                        Normal = new TAttribute(Color.BrightYellow, GlobalScheme.Normal.Background),
                        Focus = new TAttribute(Color.BrightYellow, GlobalScheme.Focus.Background),

                    };

                    _isInitalized = true;
                }
            }
        }
    }
}