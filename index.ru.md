# Платформа Связей ([english version](index.md))
Целостная система для хранения и обработки информации (в разработке), основанная на [ассоциативной модели данных](https://ru.wikipedia.org/wiki/%D0%90%D1%81%D1%81%D0%BE%D1%86%D0%B8%D0%B0%D1%82%D0%B8%D0%B2%D0%BD%D0%B0%D1%8F_%D0%BC%D0%BE%D0%B4%D0%B5%D0%BB%D1%8C_%D0%B4%D0%B0%D0%BD%D0%BD%D1%8B%D1%85).

## Требования
* [Операционная система](https://ru.wikipedia.org/wiki/%D0%9E%D0%BF%D0%B5%D1%80%D0%B0%D1%86%D0%B8%D0%BE%D0%BD%D0%BD%D0%B0%D1%8F_%D1%81%D0%B8%D1%81%D1%82%D0%B5%D0%BC%D0%B0) [Linux](https://ru.wikipedia.org/wiki/Linux), [macOS](https://ru.wikipedia.org/wiki/MacOS) или [Windows](https://ru.wikipedia.org/wiki/Windows).
* [.NET Core](https://www.microsoft.com/net) [SDK](https://ru.wikipedia.org/wiki/SDK) версии 2.2 или выше.
* [MonoDevelop](https://www.monodevelop.com/), [Visual Studio](https://visualstudio.microsoft.com) или любая другая [ИСР](https://ru.wikipedia.org/wiki/%D0%98%D0%BD%D1%82%D0%B5%D0%B3%D1%80%D0%B8%D1%80%D0%BE%D0%B2%D0%B0%D0%BD%D0%BD%D0%B0%D1%8F_%D1%81%D1%80%D0%B5%D0%B4%D0%B0_%D1%80%D0%B0%D0%B7%D1%80%D0%B0%D0%B1%D0%BE%D1%82%D0%BA%D0%B8) или просто [текстовый редактор](https://ru.wikipedia.org/wiki/%D0%A2%D0%B5%D0%BA%D1%81%D1%82%D0%BE%D0%B2%D1%8B%D0%B9_%D1%80%D0%B5%D0%B4%D0%B0%D0%BA%D1%82%D0%BE%D1%80).

## NuGet пакеты Платформы Связей

### Основные пакеты

#### [Platform.Data](https://linksplatform.github.io/Data)
Общие [интерфейсы](https://ru.wikipedia.org/wiki/%D0%98%D0%BD%D1%82%D0%B5%D1%80%D1%84%D0%B5%D0%B9%D1%81_(%D0%BE%D0%B1%D1%8A%D0%B5%D0%BA%D1%82%D0%BD%D0%BE-%D0%BE%D1%80%D0%B8%D0%B5%D0%BD%D1%82%D0%B8%D1%80%D0%BE%D0%B2%D0%B0%D0%BD%D0%BD%D0%BE%D0%B5_%D0%BF%D1%80%D0%BE%D0%B3%D1%80%D0%B0%D0%BC%D0%BC%D0%B8%D1%80%D0%BE%D0%B2%D0%B0%D0%BD%D0%B8%D0%B5)) и [классы](https://ru.wikipedia.org/wiki/%D0%9A%D0%BB%D0%B0%D1%81%D1%81_(%D0%BF%D1%80%D0%BE%D0%B3%D1%80%D0%B0%D0%BC%D0%BC%D0%B8%D1%80%D0%BE%D0%B2%D0%B0%D0%BD%D0%B8%D0%B5)) для [Дуплетов](https://linksplatform.github.io/Data.Doublets/README.ru.html) и [Триплетов](https://linksplatform.github.io/Data.Triplets).

#### [Platform.Data.Doublets](https://linksplatform.github.io/Data.Doublets/README.ru.html)
#### [Platform.Data.Triplets](https://linksplatform.github.io/Data.Triplets)
#### [Platform.Data.Triplets.Kernel](https://linksplatform.github.io/Data.Triplets.Kernel)

### Вспомогательные пакеты

#### [Platform.Data.Memory](https://linksplatform.github.io/Memory)

[Библиотека классов](https://ru.wikipedia.org/wiki/%D0%91%D0%B8%D0%B1%D0%BB%D0%B8%D0%BE%D1%82%D0%B5%D0%BA%D0%B0_(%D0%BF%D1%80%D0%BE%D0%B3%D1%80%D0%B0%D0%BC%D0%BC%D0%B8%D1%80%D0%BE%D0%B2%D0%B0%D0%BD%D0%B8%D0%B5)) Platform.Data.Memory содержит классы для упрощения [управления памятью](https://ru.wikipedia.org/wiki/%D0%A3%D0%BF%D1%80%D0%B0%D0%B2%D0%BB%D0%B5%D0%BD%D0%B8%D0%B5_%D0%BF%D0%B0%D0%BC%D1%8F%D1%82%D1%8C%D1%8E). Там вы можете найти множество реализаций интерфейса [IMemory](https://linksplatform.github.io/Memory/api/Platform.Memory.IMemory.html).

Доступ к данным может осуществляться через [указатель](https://linksplatform.github.io/Memory/api/Platform.Memory.IDirectMemory.html) или через [индексатор](https://linksplatform.github.io/Memory/api/Platform.Memory.IArrayMemory-1.html) и данные могут храниться в [энергозависимой памяти](https://ru.wikipedia.org/wiki/%D0%AD%D0%BD%D0%B5%D1%80%D0%B3%D0%BE%D0%B7%D0%B0%D0%B2%D0%B8%D1%81%D0%B8%D0%BC%D0%B0%D1%8F_%D0%BF%D0%B0%D0%BC%D1%8F%D1%82%D1%8C):
* [HeapResizableDirect](https://linksplatform.github.io/Memory/api/Platform.Memory.HeapResizableDirectMemory.html),
* [ArrayMemory](https://linksplatform.github.io/Memory/api/Platform.Memory.ArrayMemory-1.html)

или в [энергонезависимой памяти](https://ru.wikipedia.org/wiki/%D0%AD%D0%BD%D0%B5%D1%80%D0%B3%D0%BE%D0%BD%D0%B5%D0%B7%D0%B0%D0%B2%D0%B8%D1%81%D0%B8%D0%BC%D0%B0%D1%8F_%D0%BF%D0%B0%D0%BC%D1%8F%D1%82%D1%8C):
* [FileMappedResizableDirectMemory](https://linksplatform.github.io/Memory/api/Platform.Memory.FileMappedResizableDirectMemory.html),
* [TemporaryFileMappedResizableDirectMemory](https://linksplatform.github.io/Memory/api/Platform.Memory.TemporaryFileMappedResizableDirectMemory.html),
* [FileArrayMemory](https://linksplatform.github.io/Memory/api/Platform.Memory.FileArrayMemory-1.html).

#### [Platform.Data.Communication](https://linksplatform.github.io/Communication)
#### [Platform.Collections.Methods](https://linksplatform.github.io/Collections.Methods)
#### [Platform.IO](https://linksplatform.github.io/IO)
#### [Platform.Unsafe](https://linksplatform.github.io/Unsafe)
#### [Platform.Numbers](https://linksplatform.github.io/Numbers)
#### [Platform.Converters](https://linksplatform.github.io/Converters)
#### [Platform.Scopes](https://linksplatform.github.io/Scopes)
#### [Platform.Singletons](https://linksplatform.github.io/Singletons)
#### [Platform.Reflection](https://linksplatform.github.io/Reflection)
#### [Platform.Threading](https://linksplatform.github.io/Threading)
#### [Platform.Collections](https://linksplatform.github.io/Collections)
#### [Platform.Diagnostics](https://linksplatform.github.io/Diagnostics)
#### [Platform.Counters](https://linksplatform.github.io/Counters)
#### [Platform.Setters](https://linksplatform.github.io/Setters)
#### [Platform.Comparers](https://linksplatform.github.io/Comparers)
#### [Platform.Random](https://linksplatform.github.io/Random)
#### [Platform.Timestamps](https://linksplatform.github.io/Timestamps)
#### [Platform.Ranges](https://linksplatform.github.io/Ranges)
#### [Platform.Disposables](https://linksplatform.github.io/Disposables)
#### [Platform.Exceptions](https://linksplatform.github.io/Exceptions)
#### [Platform.Interfaces](https://linksplatform.github.io/Interfaces)
