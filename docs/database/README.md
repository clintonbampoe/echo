# Database Schema

This folder holds the schema definitions for the Echo database:
- `echo_schema.dbml`
- `echo_schema.dbdiagram`

## Purpose

The DBML file defines the database schema in a text format.
The DBDiagram file stores a diagram-export version of the same schema.

## Viewing the schema in VS Code

Install a DBML extension from the Extensions view:
   - `DBML`
   - `DBML Preview`
   - I'd recommend this extension [DbDiagram](https://marketplace.visualstudio.com/items?itemName=dbdiagram.dbdiagram-vscode)

## How to inspect

- Open `echo_schema.dbml`.
- Use the Command Palette:
  - `Ctrl+Shift+P`
  - Run `DBML: Open Preview` or `DBML Preview`
- If available, open `echo_schema.dbdiagram` for the diagram file too.

## Editing

- Update `echo_schema.dbml` for schema changes.
- Save the file and refresh the preview.
- Keep `echo_schema.dbdiagram` in sync if it is used as the exported diagram.

