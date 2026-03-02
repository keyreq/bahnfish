# Connecting Claude Code (CLI) to MCP Servers

## Understanding the Difference

### Claude Desktop vs Claude Code

**Claude Desktop** (GUI App):
- Config: `%APPDATA%\Claude\claude_desktop_config.json`
- Loads MCP servers automatically on startup
- Managed through JSON configuration

**Claude Code** (CLI Tool):
- Different configuration system
- May use `~/.config/claude-code/` or project-specific config
- Integration may work differently

---

## Current Status

✅ **MCP Server Installed**: `/c/Users/larry/mcp-servers/yt-analysis-mcp`
✅ **Claude Desktop Config**: Set up correctly
❓ **Claude Code Access**: Needs investigation

---

## Method 1: Check Claude Code Config Location

```bash
# Find Claude Code config directory
ls -la ~/.config/claude-code/ 2>/dev/null
ls -la ~/AppData/Roaming/claude-code/ 2>/dev/null
```

---

## Method 2: Environment Variable Approach

Since Claude Code might not load Desktop MCP configs, we can try running the MCP server directly:

```bash
# Set environment variable and test
export GEMINI_API_KEY="AIzaSyDFV_WgNRB1kQ3xDFY4bzLKkGc_SR54I2U"
node C:\Users\larry\mcp-servers\yt-analysis-mcp\dist\index.js
```

---

## Method 3: Use Claude Desktop Instead

**For YouTube video analysis, use Claude Desktop:**

1. Open Claude Desktop
2. The MCP server will automatically load
3. You can then ask: "Analyze https://www.youtube.com/watch?v=..."
4. The YouTube analysis tools will be available

---

## Method 4: Direct yt-dlp Approach (Current Session)

Since we're in Claude Code CLI now, let's use yt-dlp directly:

```bash
# Install yt-dlp
pip install yt-dlp

# Get video info
yt-dlp --get-title --get-description "https://www.youtube.com/watch?v=2RzYCMCS8Zk"

# Get transcript/subtitles
yt-dlp --write-auto-sub --skip-download "https://www.youtube.com/watch?v=2RzYCMCS8Zk"
```

---

## Recommendation

**For This Session** (Claude Code):
- I've already analyzed Cast n Chill and Dredge using web research
- The analysis in VIDEO_ANALYSIS.md is comprehensive
- We can proceed with development based on those findings

**For Future YouTube Analysis** (Claude Desktop):
- Use Claude Desktop with the MCP server we configured
- It will give you direct video analysis capabilities
- More convenient for ongoing video research

---

## Alternative: Manual Video Review

You can also:
1. Watch the videos yourself
2. Take notes on mechanics, timing, visuals
3. Share observations with me
4. I'll integrate them into the design docs

This is often the most thorough approach for game design research anyway!

---

## Current Progress

Based on web research and game documentation, I've created:

✅ **VIDEO_ANALYSIS.md** - Comprehensive analysis of:
- Cast n Chill mechanics and features
- Dredge mechanics and horror elements
- Comparative analysis
- New features to add (pet companion, enhanced hazards)
- Design adjustments needed
- Timing and pacing notes
- Audio/visual design notes

**Next**: Should I update GAME_DESIGN.md with the new features discovered (pet companion, fish-stealing mechanics, etc.)?
