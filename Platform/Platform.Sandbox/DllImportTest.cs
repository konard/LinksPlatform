using System;
using System.Runtime.InteropServices;

namespace Platform.Sandbox
{
    /// <summary>
    /// Представляет тест, который позволит определить необходимо ли указывать расширение файла при подключении библиотеки.
    ///
    /// РЕШЕНИЕ (Issue #128):
    /// Начиная с .NET Core, НЕ нужно указывать расширение .dll в именах библиотек.
    /// Runtime автоматически ищет правильное расширение для каждой платформы:
    /// - Windows: ищет "library.dll"
    /// - Linux: ищет "library.so" и "liblibrary.so"
    /// - macOS: ищет "library.dylib" и "liblibrary.dylib"
    ///
    /// Рекомендации для кроссплатформенной разработки:
    /// 1. Указывайте имя библиотеки БЕЗ расширения и префикса:
    ///    [DllImport("mylibrary")] - правильно
    ///    [DllImport("mylibrary.dll")] - НЕ рекомендуется
    ///
    /// 2. Используйте константу для имени библиотеки:
    ///    private const string DllName = "mylibrary";
    ///
    /// 3. Для сложных сценариев используйте NativeLibrary.SetDllImportResolver
    ///    (доступно в .NET Core 3.1+)
    ///
    /// Ссылки:
    /// - https://learn.microsoft.com/en-us/dotnet/standard/native-interop/native-library-loading
    /// - https://github.com/linksplatform/Data.Triplets/blob/master/csharp/Platform.Data.Triplets/Link.cs#L33
    /// </summary>
    public class DllImportTest
    {
        /// <summary>
        /// Пример правильного использования DllImport для кроссплатформенной совместимости.
        /// Имя библиотеки указано БЕЗ расширения .dll - runtime сам добавит нужное расширение.
        /// </summary>
        [DllImport("user32", CharSet = CharSet.Unicode)]
        public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

        /// <summary>
        /// Пример использования константы для имени библиотеки.
        /// Это рекомендуемый подход для реальных проектов.
        /// </summary>
        private const string NativeLibraryName = "user32";

        [DllImport(NativeLibraryName, CharSet = CharSet.Unicode)]
        private static extern int MessageBoxW(IntPtr hWnd, String text, String caption, uint type);

        public static void Test()
        {
            // Call the MessageBox function using platform invoke.
            MessageBox(new IntPtr(0), "Hello World!", "Hello Dialog", 0);
        }
    }
}
