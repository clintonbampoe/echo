# DTOs (Data Transfer Objects)

DTOs define exactly what data comes in (requests) and what goes out (responses).
They keep internal entity details separate from what the API exposes.

## DTO Types

Each entity has up to four DTOs:

| DTO               | Used for                                     |
| ----------------- | -------------------------------------------- |
| `CreateDto`       | The body of a POST request                   |
| `UpdateDto`       | The body of a PUT request                    |
| `ResponseDto`     | The full detail view returned by GET by ID   |
| `ListResponseDto` | The compact view returned in paginated lists |

`ListResponseDto` intentionally returns fewer fields than `ResponseDto`.
List views only need enough to identify a record; full details are only needed when viewing a single item.

## DTO Interfaces

| Interface             | Requirement                            |
| --------------------- | -------------------------------------- |
| `ICreateDto<T>`       | Must include `CongregationId`          |
| `IUpdateDto<T>`       | Must include `CongregationId`          |
| `IResponseDto<T>`     | Must include `Id`                      |
| `IListResponseDto<T>` | Marker interface for list DTOs         |
| `ISoftDeleteDto<T>`   | Must include `Id` and `CongregationId` |

`ICreateDto<T>` requires `CongregationId` on every create DTO.
This ensures every new record is explicitly tied to a congregation at creation time.
