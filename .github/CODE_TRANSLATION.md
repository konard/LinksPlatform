# Code Translation Workflows

This repository includes automated workflows for translating code between different programming languages.

## Available Workflows

### 1. Code Translation (C# ↔ VB.NET)

**Workflow:** `.github/workflows/code-translation.yml`

This workflow uses [ICSharpCode.CodeConverter](https://github.com/icsharpcode/CodeConverter), a Roslyn-based tool for converting code between C# and VB.NET.

#### How to Use

1. Go to **Actions** → **Code Translation**
2. Click **Run workflow**
3. Configure the translation:
   - **Source language:** Choose `csharp` or `vb`
   - **Target language:** Choose `csharp` or `vb` (must be different from source)
   - **Source path:** Path to the file or directory to translate (default: `Platform`)
   - **Create PR:** Whether to automatically create a pull request with the results

4. The workflow will:
   - Install the CodeConverter tool
   - Translate the specified code
   - Upload the translated code as an artifact
   - Optionally create a pull request with the changes

#### Features

- ✅ Accurate translation using Roslyn compiler
- ✅ Supports entire projects and directories
- ✅ Automatic pull request creation
- ✅ Translation artifacts saved for 30 days

#### Example Use Cases

- Convert legacy VB.NET code to modern C#
- Provide C# examples alongside VB.NET code
- Support developers working in different .NET languages

---

### 2. AI-Powered Code Translation

**Workflow:** `.github/workflows/ai-code-translation.yml`

This workflow uses AI (OpenAI GPT-4) to translate code between multiple programming languages.

#### Supported Languages

- Python
- JavaScript / TypeScript
- Java
- C# / VB.NET
- C++
- Go
- Rust
- Ruby
- PHP
- Swift
- Kotlin

#### Setup

To use AI-powered translation, you need to:

1. Add an `OPENAI_API_KEY` secret to your repository:
   - Go to **Settings** → **Secrets and variables** → **Actions**
   - Create a new secret named `OPENAI_API_KEY`
   - Add your OpenAI API key as the value

2. Ensure you have OpenAI API credits available

#### How to Use

1. Go to **Actions** → **AI-Powered Code Translation**
2. Click **Run workflow**
3. Configure the translation:
   - **Source file:** Path to the file to translate
   - **Target language:** Select from the supported languages
   - **Use OpenAI:** Enable to use AI translation (requires API key)

4. The workflow will:
   - Read the source file
   - Use GPT-4 to translate the code
   - Create a pull request with the translated code

#### Features

- ✅ Support for 12+ programming languages
- ✅ Intelligent translation preserving logic and style
- ✅ Automatic pull request creation
- ✅ Comments and documentation are also translated

#### ⚠️ Important Notes

- AI translation is not perfect and should be reviewed carefully
- Test the translated code thoroughly before merging
- Check for:
  - Syntax errors
  - Logic preservation
  - Framework-specific features
  - Dependencies and imports
  - Language-specific idioms

---

## Alternative Translation Tools

If you prefer manual or offline translation, consider these tools:

### For C# ↔ VB.NET
- **ICSharpCode.CodeConverter** - Command line tool
  ```bash
  dotnet tool install -g ICSharpCode.CodeConverter.CLI
  codeconverter --source MyProject.csproj --target-language vb
  ```

- **Visual Studio Extension** - Built-in conversion in VS

### For Multiple Languages
- **GitHub Copilot** - Includes "translate this code" feature for 60+ languages
- **TransCoder** - Facebook AI Research's unsupervised translation tool
- **AI Code Translator** - ML-based translation with automated testing

---

## Related Issues

- #168 - GitHub extension/app for automatic language translation
- #119 - Translate Everything
- #488 - Programming languages translation

---

## Contributing

To improve these workflows:

1. Test translations and report issues
2. Suggest additional language pairs
3. Improve translation accuracy
4. Add support for more translation tools

---

## License

These workflows are part of the LinksPlatform project and follow the same license terms.
