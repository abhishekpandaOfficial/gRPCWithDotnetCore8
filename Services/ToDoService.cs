using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ToDoGrpc.Data;
using ToDoGrpc.Models;

namespace ToDoGrpc.Services;

public class ToDoService: ToDoIt.ToDoItBase
{
    private readonly ApplicationDbContext _dbContext;

    public ToDoService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    // Create Service
    public override async Task<CreateToDoResponse> CreateToDo(CreateToDoRequest request, ServerCallContext context)
    {
        if (request.Title == string.Empty || request.Description == string.Empty)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Title and Description are required"));
        }

        var todoitem = new ToDoItem()
        {
            Title = request.Title,
            Description = request.Description
        };

        await _dbContext.AddAsync(todoitem);
        await _dbContext.SaveChangesAsync();
        return await Task.FromResult(new CreateToDoResponse()
        {
            Id = todoitem.Id
        });
    }
    
    // Read Single Service

    public override async Task<ReadToDoResponse> ReadToDo(ReadToDoRequest request, ServerCallContext context)
    {
        if (request.Id <= 0)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Id is required and Id must be greater than zero"));
        }

        var todoitem = await _dbContext.ToDoItems.FirstOrDefaultAsync(t => t.Id == request.Id);
        if (todoitem != null)
        {
            return await Task.FromResult(new ReadToDoResponse()
            {
                Id = todoitem.Id,
                Title = todoitem.Title,
                Description = todoitem.Description,
                ToDoStatus = todoitem.ToDoStatus
            });
        }

        throw new RpcException(new Status(StatusCode.NotFound, $"No Task with ID : {request.Id} was found"));
    }
    
    // Read All Lists of To Do Items
    public override async Task<GetAllResponse> ReadListToDo(GetAllRequest request, ServerCallContext context)
    {
        var response = new GetAllResponse();
        var todoitems = await _dbContext.ToDoItems.ToListAsync();
        foreach (var todo in todoitems)
        {
            response.ToDo.Add(new ReadToDoResponse()
            {
                
                Id = todo.Id,
                Title = todo.Title,
                Description = todo.Description,
                ToDoStatus = todo.ToDoStatus
            });
        }
        return await Task.FromResult(response);
    }
    
    // Update To Do Items 
    public override async Task<UpdateToDoResponse> UpdateToDo(UpdateToDoRequest request, ServerCallContext context)
    {
        if (request.Id <= 0 || request.Title == string.Empty || request.Description == string.Empty)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "You must provide Title & Description and ID should not be  Zero or Negative"));
        }

        var todoitem = await _dbContext.ToDoItems.FirstOrDefaultAsync(t => t.Id == request.Id);
        if (todoitem == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"No Task with ID : {request.Id} was found"));
        }

        todoitem.Title = request.Title;
        todoitem.Description = request.Description;
        todoitem.ToDoStatus = request.ToDoStatus;
        await _dbContext.SaveChangesAsync();

        return await Task.FromResult(new UpdateToDoResponse()
        {
            Id = todoitem.Id
        });
    }
    
    // Delete To Do Items 
    public override async Task<DeleteToDoResponse> DeleteToDo(DeleteToDoRequest request, ServerCallContext context)
    {
        if (request.Id <= 0)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Id is required and Id must be greater than zero"));
        }

        var todoitem = await _dbContext.ToDoItems.FirstOrDefaultAsync(t => t.Id == request.Id);
        if (todoitem == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"No Task with ID : {request.Id} was found"));
        }

        _dbContext.Remove(todoitem);
        await _dbContext.SaveChangesAsync();
        return await Task.FromResult(new DeleteToDoResponse()
        {
            Id = todoitem.Id
        });
    }
    
} 