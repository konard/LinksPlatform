using System;
using System.Runtime.InteropServices;

namespace Platform.Experiments
{
    /// <summary>
    /// Эксперимент для демонстрации кроссплатформенной загрузки нативных библиотек.
    /// Связан с Issue #128: проверка возможности удаления зависимости от ".dll" в именах библиотек.
    /// </summary>
    public class CrossPlatformDllImportExample
    {
        // ============================================================================
        // ПРАВИЛЬНЫЙ ПОДХОД: Имя библиотеки БЕЗ расширения
        // ============================================================================

        /// <summary>
        /// Константа для имени библиотеки БЕЗ расширения.
        /// .NET Runtime автоматически добавит правильное расширение:
        /// - Windows: mylibrary.dll
        /// - Linux: libmylibrary.so или mylibrary.so
        /// - macOS: libmylibrary.dylib или mylibrary.dylib
        /// </summary>
        private const string LibraryName = "mylibrary";

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int MyFunction();

        // ============================================================================
        // НЕПРАВИЛЬНЫЙ ПОДХОД: Имя библиотеки С расширением (не кроссплатформенно)
        // ============================================================================

        // НЕ ДЕЛАЙТЕ ТАК:
        // [DllImport("mylibrary.dll", CallingConvention = CallingConvention.Cdecl)]
        // private static extern int MyFunction();
        // Это будет работать только на Windows!

        // ============================================================================
        // ПРОДВИНУТЫЙ ПОДХОД: Использование NativeLibrary (.NET Core 3.1+)
        // ============================================================================

        /// <summary>
        /// Демонстрация использования NativeLibrary для динамической загрузки
        /// с полным контролем над процессом поиска библиотеки.
        /// </summary>
        public static void DynamicLibraryLoadingExample()
        {
            IntPtr handle = IntPtr.Zero;

            try
            {
                // Автоматический поиск библиотеки с использованием стандартных правил
                handle = NativeLibrary.Load("mylibrary");

                Console.WriteLine("Библиотека успешно загружена!");

                // Получение указателя на функцию
                if (NativeLibrary.TryGetExport(handle, "MyFunction", out IntPtr functionPtr))
                {
                    Console.WriteLine("Функция найдена!");
                }
            }
            catch (DllNotFoundException ex)
            {
                Console.WriteLine($"Библиотека не найдена: {ex.Message}");
            }
            finally
            {
                if (handle != IntPtr.Zero)
                {
                    NativeLibrary.Free(handle);
                }
            }
        }

        // ============================================================================
        // ЭКСПЕРТНЫЙ ПОДХОД: Кастомный резолвер для сложных сценариев
        // ============================================================================

        /// <summary>
        /// Демонстрация использования SetDllImportResolver для полного контроля
        /// над загрузкой библиотек, включая поддержку нестандартных путей.
        /// </summary>
        public static void SetupCustomResolver()
        {
            NativeLibrary.SetDllImportResolver(
                typeof(CrossPlatformDllImportExample).Assembly,
                (libraryName, assembly, searchPath) =>
                {
                    Console.WriteLine($"Resolving library: {libraryName}");

                    // Кастомная логика поиска библиотеки
                    if (libraryName == "mylibrary")
                    {
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            return NativeLibrary.Load("mylibrary.dll", assembly, searchPath);
                        }
                        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        {
                            return NativeLibrary.Load("libmylibrary.so", assembly, searchPath);
                        }
                        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                        {
                            return NativeLibrary.Load("libmylibrary.dylib", assembly, searchPath);
                        }
                    }

                    // Использовать стандартный механизм поиска
                    return IntPtr.Zero;
                }
            );
        }

        // ============================================================================
        // ИНФОРМАЦИЯ О ПЛАТФОРМЕ
        // ============================================================================

        /// <summary>
        /// Вспомогательный метод для вывода информации о текущей платформе.
        /// </summary>
        public static void PrintPlatformInfo()
        {
            Console.WriteLine("=== Platform Information ===");
            Console.WriteLine($"OS: {RuntimeInformation.OSDescription}");
            Console.WriteLine($"Architecture: {RuntimeInformation.OSArchitecture}");
            Console.WriteLine($"Framework: {RuntimeInformation.FrameworkDescription}");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.WriteLine("Platform: Windows (expects .dll)");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Console.WriteLine("Platform: Linux (expects .so, with or without 'lib' prefix)");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Console.WriteLine("Platform: macOS (expects .dylib, with or without 'lib' prefix)");
            }

            Console.WriteLine("============================");
        }

        /// <summary>
        /// Точка входа для тестирования примеров.
        /// </summary>
        public static void RunExperiments()
        {
            Console.WriteLine("Cross-Platform DllImport Example - Issue #128");
            Console.WriteLine();

            PrintPlatformInfo();
            Console.WriteLine();

            Console.WriteLine("ВЫВОД:");
            Console.WriteLine("- Начиная с .NET Core, НЕ нужно указывать расширение .dll");
            Console.WriteLine("- Runtime автоматически ищет правильное расширение для платформы");
            Console.WriteLine("- Используйте простое имя без расширения: [DllImport(\"mylibrary\")]");
            Console.WriteLine("- Для сложных случаев используйте NativeLibrary API");
        }
    }
}
