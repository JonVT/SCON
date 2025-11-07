# Changelog

All notable changes to this project will be documented in this file.

The format is based on Keep a Changelog, and this project adheres to Semantic Versioning.

## [1.0.0] - 2025-11-07
### Added
- HTTP API server for Stationeers with command execution and game info endpoints
- POST `/command` endpoint to execute in-game console commands
- GET `/gameinfo` endpoint for server and world information
- GET `/version` endpoint for plugin version information
- GET `/health` endpoint for health checks
- API key authentication with configurable key
- CORS support for web-based clients
- Proper handling of escaped quotes in command strings
- BepInEx plugin configuration options for host, port, and API key
