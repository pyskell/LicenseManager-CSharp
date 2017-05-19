# Purpose
This is a simple WPF application for common Portable.Licensing tasks. It provides the following functionality:

1. Generate a public/private key pair
2. Generate a Standard-type License with expiration, name, email, and company details
3. Optionally upload the generated license (at creation time) to a [LicenseServer](https://github.com/pyskell/LicenseServer). If you're not using LicenseServer then simply leave the URL and all related information below it blank.

Not to be confused with [License.Manager](https://github.com/dnauck/License.Manager) which provides more advanced functionality (and runs from a webserver)

# Dependencies
Requires Portable.Licensing (obviously) and Newtonsoft.Json

Install from NuGet via:
```
Install-Package Portable.Licensing
Install-Package Newtonsoft.Json
```

# MIT License
Copyright 2017

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
associated documentation files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS 
BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
OR OTHER DEALINGS IN THE SOFTWARE.