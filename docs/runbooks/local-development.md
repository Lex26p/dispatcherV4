# Local development runbook

## Назначение

Этот runbook фиксирует начальный локальный процесс разработки после очистки репозитория.

## Рабочий путь

Рекомендуемый локальный путь:

```text
C:\Projects\dispatcherV4
```

## Ветка

Основная ветка:

```text
master
```

Новые ветки не создаются, если пользователь явно не изменит рабочий процесс.

## Step 0A: очистка репозитория

Step 0A удаляет старое содержимое тренировочного проекта и заменяет его промышленным документационным baseline. Git history сохраняется.

Проверки после применения:

```powershell
Test-Path .\README.md
Test-Path .\PROJECT_STATE.md
Test-Path .\DISPATCHER_TECHNICAL_SPECIFICATION_AND_ROADMAP.md
Test-Path .\DISPATCHER_AI_IMPLEMENTATION_SPEC.md
Test-Path .\docs\adr\ADR-0001-architecture-baseline.md
```

## До появления solution

До Step 1 команды `dotnet restore`, `dotnet build` и `dotnet test` могут быть неприменимы, потому что `.sln` еще не создан. Это нормально для Step 0A.

## После Step 1

После создания solution стандартная проверка каждого кодового шага:

```powershell
dotnet restore .\Dispatcher.sln
```

```powershell
dotnet build .\Dispatcher.sln
```

```powershell
dotnet test .\Dispatcher.sln
```

## Git workflow

После каждого шага:

```powershell
git status
```

```powershell
git add .
```

```powershell
git commit -m "<step message>"
```

```powershell
git push origin master
```

```powershell
git rev-parse HEAD
```

Commit hash нужно прислать ИИ-агенту, чтобы он обновил рабочий контекст.
