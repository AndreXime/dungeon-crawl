# Dungeon Crawl — comandos do projeto
# Uso: just <receita>   |   just --list

set shell := ["bash", "-eu", "-o", "pipefail", "-c"]

project := "ChamberCrawl/ChamberCrawl.csproj"

default:
    @just --list

run:
    dotnet run --project ChamberCrawl

build:
    dotnet build {{project}} -c Release

test:
    dotnet test

# Pasta self-contained (jogador não precisa de .NET) + zip em dist/
publish-linux:
    mkdir -p dist
    dotnet publish {{project}} -c Release \
      -r linux-x64 \
      --self-contained true \
      -p:PublishSingleFile=false \
      -o dist/linux-x64
    rm -f dist/linux-x64.zip
    (cd dist && zip -qr linux-x64.zip linux-x64)

publish-win:
    mkdir -p dist
    dotnet publish {{project}} -c Release \
      -r win-x64 \
      --self-contained true \
      -p:PublishSingleFile=false \
      -o dist/win-x64
    rm -f dist/win-x64.zip
    (cd dist && zip -qr win-x64.zip win-x64)

publish-all: publish-linux publish-win
