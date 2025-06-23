#!/usr/bin/env bash

# Script chỉ tạo file .gitignore cho .NET solution ở thư mục hiện tại
# Chạy tại thư mục gốc của solution

cat > .gitignore << 'EOF'
# Build artifacts
bin/
obj/

# Visual Studio files
.vs/
*.user
*.suo
*.userosscache
*.sln.docstates

# Rider files
.idea/
*.sln.iml

# VS Code files
.vscode/

# User-specific files
*.mef
*.dbmdl

# Resharper
_ReSharper*/
*.DotSettings.user

# Entity Framework migrations
Migrations/

# NuGet packages
*.nupkg
packages/

# Logs
*.log

# OS files
*.DS_Store
EOF

echo ".gitignore đã được tạo ở $(pwd)/.gitignore"
