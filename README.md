# My Agent Skills

This repository contains my personal set of core skills and custom agents for coding agents. For information about the Agent Skills standard, see [agentskills.io](https://agentskills.io).

## What's Included

| Plugin | Description |
|--------|-------------|
| [makita-deal-finder](plugins/makita-deal-finder/) | Find the best Makita 18V LXT & 40V XGT solo tool deals in Austria using Geizhals price history API, willhaben.at, and eBay.de. |

## Installation

### 🚀 Plugins - Copilot CLI / Claude Code

1. Launch Copilot CLI or Claude Code
2. Add the marketplace:
   ```
   /plugin marketplace add ViktorHofer/skills
   ```
3. Install a plugin:
   ```
   /plugin install <plugin>@viktorhofer-agent-skills
   ```
4. Restart to load the new plugins
5. View available skills:
   ```
   /skills
   ```
6. View available agents:
   ```
   /agents
   ```
7. Update plugin (on demand):
   ```
   /plugin update <plugin>@viktorhofer-agent-skills
   ```

### VS Code / VS Code Insiders (Preview)

> [!IMPORTANT]  
> VS Code plugin support is a preview feature and subject to change. You may need to enable it first.

```jsonc
// settings.json
{
  "chat.plugins.enabled": true,
  "chat.plugins.marketplaces": ["dotnet/skills"]
}
```

Once configured, type `/plugins` in Copilot Chat or use the `@agentPlugins` filter in Extensions to browse and install plugins from the marketplace.

### Codex CLI

Skills in this repository follow the [agentskills.io](https://agentskills.io) open standard
and are compatible with [OpenAI Codex](https://developers.openai.com/codex/skills).

Install individual skills using the `skill-installer` CLI with the GitHub URL:

```bash
$ skill-installer install https://github.com/ViktorHofer/skills/tree/main/plugins/<plugin>/skills/<skill-name>
```

## License

See [LICENSE](LICENSE) for details.
