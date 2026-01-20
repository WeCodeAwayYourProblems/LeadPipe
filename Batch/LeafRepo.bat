@echo off
title LeafRepo updateRepo

"%USERPROFILE%\Repos\Automate\Automate.Cli\bin\Release\net8.0\Automate.Cli.exe" updateRepo -ft Leaf -a "%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\ApiRepos\LeafThreads.json"
