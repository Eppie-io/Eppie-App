# Mailto Protocol Support

Eppie now supports the `mailto:` protocol, allowing users to compose emails directly from external applications and websites.

## Features

- **Cross-Platform Support**: Works on Windows, macOS, Linux, iOS, and Android
- **RFC 6068 Based**: Supports core mailto URI fields (to, cc, bcc, subject, body)
- **Pre-filled Fields**: Automatically populates recipient, subject, body, CC, and BCC
- **Default Mail Handler**: Can be set as the system's default email application (Windows, macOS, Linux)

## Supported Mailto Formats

Eppie supports all standard mailto URI formats:

### Basic Examples
```
mailto:user@example.com
mailto:user@example.com?subject=Hello
mailto:user@example.com?subject=Hello&body=Message
```

### Advanced Examples
```
mailto:user1@example.com?to=user2@example.com&cc=cc@example.com&bcc=bcc@example.com&subject=Team%20Meeting&body=Let's%20discuss
mailto:?subject=Feedback&body=Please%20share%20your%20thoughts
```

### Supported Parameters
- `to` - Primary recipient(s)
- `cc` - Carbon copy recipient(s)
- `bcc` - Blind carbon copy recipient(s)
- `subject` - Email subject line
- `body` - Email body text

## Platform-Specific Behavior

### Windows (UWP/WinAppSDK)
- Registered via `Package.appxmanifest`
- Can be set as default mail handler in Windows Settings
- Activated via protocol activation events

### macOS
- Registered via `Info.plist` CFBundleURLTypes
- Can be set as default mail handler in System Preferences > General > Default email reader
- Handled via URL scheme callbacks

### Linux
- Registered via `.desktop` file with `x-scheme-handler/mailto`
- Can be set as default using `xdg-settings set default-url-scheme-handler mailto eppie.desktop`
- Launched with mailto URI as command-line argument

### iOS
- Registered via `Info.plist` CFBundleURLSchemes
- System automatically prompts user when first mailto link is clicked
- Launches Eppie when a `mailto:` link is tapped
- Note: opening a pre-filled compose window from `mailto:` links currently requires additional iOS-specific activation handling and may not be fully supported yet

### Android
- Registered via AndroidManifest.xml intent-filter
- System shows app chooser if multiple mail apps are installed
- Handled via MainActivity intent processing

## Setting Eppie as Default Mail Handler

### Windows
1. Open Windows Settings
2. Navigate to Apps > Default apps
3. Click "Choose default apps by protocol"
4. Find "MAILTO" in the list
5. Select Eppie

### macOS
1. Open System Preferences
2. Go to General
3. Find "Default email reader"
4. Select Eppie

### Linux
Terminal command:
```bash
xdg-settings set default-url-scheme-handler mailto eppie.desktop
```

Or use your desktop environment's default applications settings.

## Testing

You can test mailto support by:

1. Creating an HTML file with mailto links:
```html
<a href="mailto:test@example.com?subject=Test&body=Hello">Send Email</a>
```

2. Using command line (Linux/macOS):
```bash
xdg-open "mailto:test@example.com?subject=Test"
open "mailto:test@example.com?subject=Test"  # macOS
```

3. Using command line (Windows):
```powershell
start "mailto:test@example.com?subject=Test"
```

## Implementation Details

### Core Components
- **MailtoUriParser**: Parses mailto URIs according to RFC 6068
- **MailtoMessageData**: Creates NewMessageData from parsed mailto components
- **App Protocol Handlers**: Platform-specific handlers that intercept mailto activations

### Flow
1. User clicks mailto link in browser/application
2. OS launches/activates Eppie with mailto URI
3. Platform-specific handler captures URI
4. App stores URI as pending until fully initialized
5. After navigation ready, App parses mailto URI
6. App creates MailtoMessageData with extracted fields
7. App navigates to NewMessagePage with pre-filled data
8. User sees compose window ready to send

## Security Considerations

- All mailto URIs are validated before processing
- URL decoding is performed safely to prevent injection attacks
- Invalid schemes are rejected
- Malformed URIs result in graceful fallback

## Known Limitations

- Attachment parameter (`?attach=`) is not supported (not part of RFC 6068)
- Custom headers are not supported
- Maximum URL length is subject to OS/browser limitations

## Troubleshooting

### mailto links don't open Eppie
1. Check that Eppie is installed correctly
2. Verify Eppie is set as default mail handler (see above)
3. Restart the browser/application
4. On Linux, verify the desktop file is in the correct location

### Fields not pre-filled correctly
1. Verify the mailto URL is properly encoded
2. Check that special characters are URL-encoded (spaces as %20, etc.)
3. Ensure the mailto URI follows RFC 6068 standard

### Multiple mail clients open
This is expected behavior when multiple mail clients are installed. Set Eppie as the default handler to prevent this.
