# Setting Up YouTube Access for Claude Code

## Method 1: YouTube MCP Server (Recommended)

### Prerequisites
- ✅ Node.js 18+ (You have Node.js installed)
- 🔑 Gemini API key (Free tier works)

### Step 1: Get Gemini API Key
1. Go to [Google AI Studio](https://aistudio.google.com/app/apikey)
2. Sign in with your Google account
3. Click "Create API Key"
4. Copy the key (keep it secret!)

### Step 2: Install yt-analysis MCP Server

Run these commands:

```bash
# Navigate to a directory for MCP servers
cd %USERPROFILE%
mkdir mcp-servers
cd mcp-servers

# Clone the repository
git clone https://github.com/rusiaaman/yt-analysis-mcp.git
cd yt-analysis-mcp

# Install dependencies
npm install
```

### Step 3: Configure Claude Desktop

Find or create the config file at:
- Windows: `%APPDATA%\Claude\claude_desktop_config.json`
- Or: `C:\Users\larry\AppData\Roaming\Claude\claude_desktop_config.json`

Add this configuration:

```json
{
  "mcpServers": {
    "youtube-analysis": {
      "command": "node",
      "args": [
        "C:\\Users\\larry\\mcp-servers\\yt-analysis-mcp\\build\\index.js"
      ],
      "env": {
        "GEMINI_API_KEY": "YOUR_API_KEY_HERE"
      }
    }
  }
}
```

### Step 4: Restart Claude Desktop

After configuration, restart Claude Desktop completely.

### Step 5: Test

Once configured, I'll be able to:
- Get video transcripts
- Summarize content
- Extract specific information
- Answer questions about the video

**Commands you can use:**
- "Analyze this YouTube video: [URL]"
- "Get the transcript from [URL]"
- "Summarize the key points from [URL]"

---

## Method 2: Claude Code Browser Control

This method lets me control a browser directly.

### Step 1: Install Chrome Extension

Run this command in your terminal:
```bash
cloud code /chrome
```

Or manually:
1. Install the Claude Code Chrome extension
2. Enable it in Chrome

### Step 2: Connect Extension

After installation, run:
```bash
connect extension
```

Or in Claude Code:
- I should automatically detect the extension
- You may need to authorize the connection

### Step 3: Test

Once connected, I can:
- Navigate to YouTube URLs
- Take screenshots
- Extract visible information
- Interact with the page

---

## Quick Start Command Sequence

### For Method 1 (MCP Server):
```bash
# 1. Get API key from https://aistudio.google.com/app/apikey

# 2. Install the server
cd %USERPROFILE%
mkdir mcp-servers
cd mcp-servers
git clone https://github.com/rusiaaman/yt-analysis-mcp.git
cd yt-analysis-mcp
npm install

# 3. Create config directory if it doesn't exist
mkdir "%APPDATA%\Claude" 2>nul

# 4. Edit the config file (paste the JSON above with your API key)
notepad "%APPDATA%\Claude\claude_desktop_config.json"

# 5. Restart Claude Desktop
```

### For Method 2 (Browser):
```bash
# Run in terminal
cloud code /chrome
connect extension
```

---

## Which Should You Choose?

### Choose Method 1 (MCP Server) if:
- ✅ You want permanent YouTube access
- ✅ You need transcripts and detailed analysis
- ✅ You're okay getting a free Gemini API key
- ✅ You want the most reliable solution

### Choose Method 2 (Browser) if:
- ✅ You want visual screenshots
- ✅ You don't want to manage API keys
- ✅ You need to interact with YouTube UI
- ✅ You want quick setup

---

## Troubleshooting

### MCP Server Not Working?
- Check that Node.js path is correct in config
- Verify API key is valid
- Make sure Claude Desktop is fully restarted
- Check `build/index.js` exists in the yt-analysis-mcp folder

### Browser Extension Not Working?
- Verify Chrome is installed
- Check that extension is enabled
- Try restarting Claude Code
- Check browser console for errors

---

## After Setup

Once either method is working, come back and say:
- "Analyze YouTube video [URL]" (Method 1)
- "Navigate to YouTube and show me [URL]" (Method 2)

And I'll be able to extract information about:
- Cast n Chill gameplay and mechanics
- Dredge horror and fishing systems
- Any other reference videos
