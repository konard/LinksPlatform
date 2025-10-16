#ifndef __LINKS_COMMON_H__
#define __LINKS_COMMON_H__

#define false 0
#define true 1
#define bool unsigned

#include <stdint.h>
#include <inttypes.h>

// Size for basic types
// Размер для основных типов
typedef uint64_t unsigned_integer; // Unsigned integer (Беззнаковое целое число)
typedef int64_t signed_integer; // Signed integer (Целое число со знаком)
typedef unsigned_integer link_index; // Short for links' array index, unsigned integer (короткая форма для беззнакового индекса в массиве связей)

#if defined(_MSC_VER) || defined(__MINGW32__) || defined(__MINGW64__) // Для Windows: получения .exe/.obj/.dll (Visual C++/MinGW32):
#ifndef WINDOWS
#define WINDOWS
#endif
#elif defined(__linux__)  // Для Linux: получения ./.o/.so (dll):
#ifndef LINUX
#define LINUX
#define UNIX
#endif
#elif defined(__APPLE__) && defined(__MACH__)
#ifndef MACOS
#define MACOS
#define UNIX
#endif
#endif

// see http://stackoverflow.com/questions/538134/exporting-functions-from-a-dll-with-dllexport
#if defined(WINDOWS)
#if defined(LINKS_DLL_EXPORT) || defined (CORE_EXPORTS)
#define PREFIX_DLL __declspec(dllexport)
//#define public_calling_convention __stdcall
#define public_calling_convention
#else
#define PREFIX_DLL __declspec(dllimport)
#define public_calling_convention
#endif
#elif defined(UNIX)
#define PREFIX_DLL
#define public_calling_convention
#endif

#ifdef _DEBUG
#define DEBUG
#endif

#ifdef DEBUG
#include <stdio.h>
#endif

#define SUCCESS_RESULT 1
#define succeeded(x) (SUCCESS_RESULT == (x))
#define ERROR_RESULT 0
#define failed(x) (SUCCESS_RESULT != (x))


#ifdef DEBUG
    #define ERROR_MESSAGE(message)  fprintf(stderr, "%s\n\n", message);
#else
    #define ERROR_MESSAGE(message)
#endif


#ifdef DEBUG
    #define ERROR_MESSAGE_WITH_CODE(message, errorCode)  fprintf(stderr, "%s Error code: %" PRId64 ".\n\n", message, errorCode);
#else
    #define ERROR_MESSAGE_WITH_CODE(message, errorCode)
#endif

#ifdef DEBUG
    #define DEBUG_MESSAGE(message) fprintf(stdout, "%s\n", message)
#else
    #define DEBUG_MESSAGE(message)
#endif
#endif
