# Aresdoor
###  Yet another persistant backdoor for Windows, in C#
***

### Features
 * Automatically modify registry to autorun on startup
 * 100% persistant backdoor
 * Silent exception escapes (so no noisy dialogs)
 * Checks for internet connection before sending backdoor
 * Command line switches for defining server and port
 * Can add self to registry for autorun on bootup
 * Can prevent system shutdown
 * Modify process name to "hide in plain sight" (default: System)

### To-Do
 * Add password-protection
 * Create server-side application
 * Encrypt/Decrypt TCP data sent to/from client/server
 * Add support for Powershell command line
 * Fix runtime error handling

***
### Version History
 * ### v1.2
   - Fixed change directory bug for command line
   - Add option to prevent shutdown (built into the code)
   - Changed process name to "System" to help "hide in plain sight"
   - Can now insert itself into registry to autorun on bootup (use 'setStartup' in backdoored command line)
 * ### v1.1
   - Add command line switches for modifying server and port to connect back to
   - Clean up source code for easier managment
   - Added more trust in executable so browsers are unlikely to pick it up as 'untrustworthy'
 * ### v1.0
   -  Initial Release

***

### Contribution
This, along with all other projects found in the Ares framework, is an experimental tool. Full stability is
not currently guaranteed in this tool. Please report all errors by [submitting an issue](https://github.com/BlackVikingPro/aresdoor/issues/new).
You may also feel free to fix any bugs yourself, and publish a new branch containing your fix. I will review
all code changes, and then update to the master branch (along with return credit for fixing it) should the
bug fix be programatically correct. Thanks!

### Disclaimer
I am not responsible for any malicious use of this software. This is a free, and open-source software to anybody
that would like to use it, and for any groups (personal, corporate, or military).