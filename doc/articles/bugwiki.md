# Bugwiki - Encyclopedia of Bugs and Software Errors

## Introduction

Bugwiki is a comprehensive knowledge base that serves as a middle ground between Wikipedia and StackOverflow, specifically designed to help programmers and users quickly identify software errors, understand their meanings, and find related information for troubleshooting and resolution.

## Purpose

The main goals of Bugwiki are:
- Fast identification of what exactly happened when an error occurs
- Clear explanations of error messages and their meanings
- References to related information, documentation, and resources
- Common causes and solutions for known issues
- Cross-references between related errors and problems

## Structure

Each bug/error entry in Bugwiki follows a standardized structure to ensure consistency and ease of use:

### Entry Template

```
# Error Name/Code

## Summary
Brief one-line description of the error.

## Error Message
The exact error message or code that users will encounter.

## What It Means
Clear explanation of what this error indicates.

## Common Causes
- Cause 1
- Cause 2
- Cause 3

## How to Fix
Step-by-step solutions for resolving the issue.

## Related Errors
Links to similar or related errors.

## References
- Links to official documentation
- Related StackOverflow questions
- Bug reports
- Forum discussions

## Examples
Real-world code examples that trigger this error and their fixes.
```

## Example Entries

### NullPointerException (Java)

#### Summary
Attempt to use an object reference that has not been initialized or has been set to null.

#### Error Message
```
java.lang.NullPointerException
```

#### What It Means
This exception occurs when the application attempts to use a null object reference - trying to call a method, access a field, or perform any operation on an object that doesn't exist in memory.

#### Common Causes
- Object was not initialized before use
- Method returned null when a valid object was expected
- Typo in variable name leading to wrong (uninitialized) variable
- Array element accessed before initialization
- External API or database query returned null unexpectedly

#### How to Fix
1. **Initialize objects before use:**
   ```java
   String text = null;
   System.out.println(text.length()); // NullPointerException

   // Fix:
   String text = "";
   System.out.println(text.length()); // Works
   ```

2. **Check for null before use:**
   ```java
   if (object != null) {
       object.doSomething();
   }
   ```

3. **Use Optional (Java 8+):**
   ```java
   Optional<String> optional = Optional.ofNullable(getValue());
   optional.ifPresent(value -> System.out.println(value));
   ```

4. **Provide default values:**
   ```java
   String result = getValue() != null ? getValue() : "default";
   ```

#### Related Errors
- IllegalArgumentException
- NullPointerException in other languages (C#: NullReferenceException, JavaScript: Cannot read property of undefined)

#### References
- [Java Documentation: NullPointerException](https://docs.oracle.com/javase/8/docs/api/java/lang/NullPointerException.html)
- [StackOverflow: What is a NullPointerException?](https://stackoverflow.com/questions/218384/what-is-a-nullpointerexception)

---

### SegmentationFault (C/C++)

#### Summary
Program attempted to access a memory location that it does not have permission to access.

#### Error Message
```
Segmentation fault (core dumped)
```

#### What It Means
The program tried to read or write to a memory address that is outside its allocated memory space, causing the operating system to terminate the program to protect system stability.

#### Common Causes
- Dereferencing a NULL pointer
- Accessing array out of bounds
- Using freed memory (use-after-free)
- Stack overflow from infinite recursion
- Writing to read-only memory
- Buffer overflow

#### How to Fix
1. **Check for NULL before dereferencing:**
   ```c
   int* ptr = malloc(sizeof(int));
   if (ptr == NULL) {
       // Handle allocation failure
       return -1;
   }
   *ptr = 42; // Safe to use
   free(ptr);
   ```

2. **Validate array indices:**
   ```c
   int arr[10];
   int index = 15;
   if (index >= 0 && index < 10) {
       arr[index] = 100; // Safe
   }
   ```

3. **Don't use memory after freeing:**
   ```c
   int* ptr = malloc(sizeof(int));
   free(ptr);
   // ptr = NULL; // Good practice
   // *ptr = 42; // ERROR: use-after-free
   ```

4. **Use debugging tools:**
   - Valgrind for memory leak detection
   - GDB for debugging
   - AddressSanitizer (compile with `-fsanitize=address`)

#### Related Errors
- Bus Error
- Stack Overflow
- Heap Corruption
- Buffer Overflow

#### References
- [Wikipedia: Segmentation Fault](https://en.wikipedia.org/wiki/Segmentation_fault)
- [Valgrind Documentation](https://valgrind.org/docs/manual/quick-start.html)
- [GDB Tutorial](https://www.gnu.org/software/gdb/documentation/)

---

### ModuleNotFoundError (Python)

#### Summary
Python cannot find the module you are trying to import.

#### Error Message
```
ModuleNotFoundError: No module named 'module_name'
```

#### What It Means
The Python interpreter cannot locate the module you're trying to import. This means the module is either not installed, not in the Python path, or has a different name.

#### Common Causes
- Module not installed via pip
- Wrong module name (typo)
- Virtual environment not activated
- Module installed in different Python version
- Incorrect PYTHONPATH
- Circular imports

#### How to Fix
1. **Install the module:**
   ```bash
   pip install module_name
   # or for Python 3 specifically:
   pip3 install module_name
   ```

2. **Activate virtual environment:**
   ```bash
   # Linux/Mac:
   source venv/bin/activate

   # Windows:
   venv\Scripts\activate
   ```

3. **Check installed packages:**
   ```bash
   pip list
   pip show module_name
   ```

4. **Verify Python version:**
   ```bash
   python --version
   which python
   ```

5. **Add to PYTHONPATH:**
   ```python
   import sys
   sys.path.append('/path/to/module')
   ```

#### Related Errors
- ImportError
- NameError
- AttributeError (when trying to access non-existent module attribute)

#### References
- [Python Documentation: The import system](https://docs.python.org/3/reference/import.html)
- [pip Documentation](https://pip.pypa.io/en/stable/)
- [Virtual Environments Tutorial](https://docs.python.org/3/tutorial/venv.html)

---

### CORS Error (Web Development)

#### Summary
Cross-Origin Resource Sharing policy blocks the browser from loading resources from a different origin.

#### Error Message
```
Access to fetch at 'https://api.example.com' from origin 'https://mysite.com'
has been blocked by CORS policy: No 'Access-Control-Allow-Origin' header is
present on the requested resource.
```

#### What It Means
The browser's security policy prevents a web page from making requests to a different domain than the one that served the web page, unless the target server explicitly allows it through CORS headers.

#### Common Causes
- Server not configured to allow cross-origin requests
- Missing or incorrect CORS headers
- Preflight request (OPTIONS) not handled
- Credentials being sent without proper configuration
- Localhost/production environment mismatch

#### How to Fix

**Server-side fixes:**

1. **Node.js/Express:**
   ```javascript
   const cors = require('cors');
   app.use(cors());

   // Or with specific origin:
   app.use(cors({
     origin: 'https://mysite.com',
     credentials: true
   }));
   ```

2. **Python/Flask:**
   ```python
   from flask_cors import CORS

   app = Flask(__name__)
   CORS(app)

   # Or specific routes:
   @app.route('/api/data')
   @cross_origin()
   def get_data():
       return jsonify(data)
   ```

3. **Apache (.htaccess):**
   ```apache
   Header set Access-Control-Allow-Origin "*"
   Header set Access-Control-Allow-Methods "GET, POST, PUT, DELETE, OPTIONS"
   Header set Access-Control-Allow-Headers "Content-Type"
   ```

4. **Nginx:**
   ```nginx
   add_header 'Access-Control-Allow-Origin' '*';
   add_header 'Access-Control-Allow-Methods' 'GET, POST, OPTIONS';
   add_header 'Access-Control-Allow-Headers' 'Content-Type';
   ```

**Development workarounds:**

1. **Use a proxy in development:**
   ```javascript
   // package.json (React)
   "proxy": "https://api.example.com"
   ```

2. **Browser extensions (development only):**
   - CORS Unblock
   - Allow CORS

3. **Chrome with disabled security (development only, dangerous):**
   ```bash
   chrome --disable-web-security --user-data-dir=/tmp/chrome_dev
   ```

#### Related Errors
- Mixed Content Error
- CSP (Content Security Policy) violations
- 403 Forbidden
- Preflight request errors

#### References
- [MDN: CORS](https://developer.mozilla.org/en-US/docs/Web/HTTP/CORS)
- [CORS Explained](https://www.codecademy.com/articles/what-is-cors)
- [Enable CORS](https://enable-cors.org/)

---

## Contributing to Bugwiki

To add a new bug/error entry:

1. Follow the entry template structure
2. Include practical examples with code
3. Provide accurate references to official documentation
4. Link related errors for cross-referencing
5. Keep explanations clear and concise
6. Include common solutions that have been verified

## Categories

Bugwiki entries can be organized by:
- **Programming Language**: Java, Python, C/C++, JavaScript, etc.
- **Platform**: Web, Mobile, Desktop, Embedded
- **Error Type**: Runtime, Compile-time, Configuration, Network, etc.
- **Severity**: Critical, High, Medium, Low
- **Component**: Database, Authentication, File System, Memory, etc.

## Search and Navigation

Users should be able to:
- Search by error message or code
- Browse by category
- Filter by programming language
- View related errors
- Sort by frequency or recency

## Future Enhancements

- Community voting on solutions
- Solution effectiveness ratings
- Error frequency tracking
- Machine learning for error similarity detection
- Integration with IDEs and development tools
- Multilingual support
- API for programmatic access
