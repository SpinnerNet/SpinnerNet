# 🧹 Temporary File Management Guidelines

## ❌ **NEVER DO THIS**
```bash
# DON'T save files in project directories
mcp__safari__take_screenshot({ filename: "test-screenshot" })
cd src && zip -r deployment.zip publish/
```

## ✅ **ALWAYS DO THIS**
```bash
# Use temp folder for ALL temporary files
mcp__safari__take_screenshot({ filename: "/tmp/spinnernet-temp/test-screenshot" })
cd /tmp/spinnernet-temp && zip -r deployment.zip publish/
```

## 🧽 **Cleanup Commands**

### Quick Cleanup
```bash
rm -rf /tmp/spinnernet-temp/* && mkdir -p /tmp/spinnernet-temp
```

### Full Repository Cleanup
```bash
.claude/scripts/cleanup-temp-files.sh
```

## 🚨 **Common Mistake Patterns**

These files should NEVER appear in git:
- `after-form-fill`
- `form-filled-test`
- `deployment.zip`
- `deployment-clean.zip`
- `*.png` (in root/src)
- `*screenshot*`
- `*-test*`
- `step1-completed`
- `manual-step3-navigation`

## 📝 **Workflow**

1. **Before testing**: `mkdir -p /tmp/spinnernet-temp`
2. **During testing**: Use `/tmp/spinnernet-temp/` for ALL files
3. **After testing**: Run `.claude/scripts/cleanup-temp-files.sh`
4. **Before commits**: Verify no temp files with `git status`

## 🎯 **Remember**

> **Keep the git repository clean at all times!**
> Temporary files should never be committed or left in project directories.