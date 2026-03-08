# SmartSpec

## 📖 專案介紹 (Project Overview)
這是一個專為企業環境設計的集中式後端 API 系統，旨在解決企業內跨裝置、跨終端需存取同一個系統時的資料同步與整合問題。透過單一的 API 服務，確保各個客戶端（不同電腦）都能獲得一致的業務邏輯與資料狀態。本專案的核心業務主要用於處理**產品規格的建檔與管理**。

## ✨ 核心特色 (Features)
- 採用 **Clean Architecture** (整潔架構) 進行開發，實現關注點分離。
- 使用 `.NET Core / C#` 作為核心開發語言。
- 整合了 HTML/CSS 提供基本的前端介面。
- 支援 **Docker** 容器化部署，能快速建立統一的開發與運行環境。

## 🛠️ 技術堆疊 (Tech Stack)
- **後端**: C#, ASP.NET Core API
- **前端**: HTML, CSS
- **架構設計**: Clean Architecture (Domain, Application, Infrastructure, Web API)
- **環境與部署**: Docker, Docker Compose

## 🚀 如何在本地端運行 (Getting Started)

這份指南將協助你在本機電腦上啟動並運行此專案。

### 先決條件 (Prerequisites)
請確保您的電腦已安裝以下軟體：
- [.NET SDK](https://dotnet.microsoft.com/download) (請確認您的版本，如 .NET 8)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### 啟動步驟 (Installation & Run)
1. 複製此專案到本地端：
   ```bash
   git clone https://github.com/ikai-0706/SmartSpec.git
