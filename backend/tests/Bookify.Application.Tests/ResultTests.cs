using FluentAssertions;
using Bookify.Application.Common;

namespace Bookify.Application.Tests;

public class ResultTests
{
    [Fact]
    public void Success_WithData_SetsIsSuccess()
    {
        var result = Result<string>.Success("data");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be("data");
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_WithError_SetsIsFailure()
    {
        var result = Result<string>.Failure("Something went wrong", "ERR_001");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Something went wrong");
        result.ErrorCode.Should().Be("ERR_001");
        result.Data.Should().Be(default);
    }

    [Fact]
    public void ApiResponse_Ok_ReturnsSuccess()
    {
        var response = ApiResponse<string>.Ok("data", "Success message");

        response.Success.Should().BeTrue();
        response.Data.Should().Be("data");
        response.Message.Should().Be("Success message");
    }

    [Fact]
    public void ApiResponse_Fail_ReturnsFailure()
    {
        var response = ApiResponse<string>.Fail("Error occurred", new { field = "error" });

        response.Success.Should().BeFalse();
        response.Message.Should().Be("Error occurred");
        response.Errors.Should().NotBeNull();
    }

    [Fact]
    public void PaginatedList_CalculatesTotalPages()
    {
        var paginated = new PaginatedList<int>(new[] { 1, 2, 3 }.ToList(), 1, 10, 3);

        paginated.TotalPages.Should().Be(1);
        paginated.HasNextPage.Should().BeFalse();
        paginated.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void PaginatedList_MultiplePages_CalculatesCorrectly()
    {
        var paginated = new PaginatedList<int>(
            Enumerable.Range(1, 10).ToList(), 2, 10, 25);

        paginated.Page.Should().Be(2);
        paginated.PageSize.Should().Be(10);
        paginated.TotalCount.Should().Be(25);
        paginated.TotalPages.Should().Be(3);
        paginated.HasNextPage.Should().BeTrue();
        paginated.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void PaginatedList_FirstPage_HasNoPrevious()
    {
        var paginated = new PaginatedList<int>(
            Enumerable.Range(1, 10).ToList(), 1, 10, 25);

        paginated.HasPreviousPage.Should().BeFalse();
        paginated.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void PaginatedList_LastPage_HasNoNext()
    {
        var paginated = new PaginatedList<int>(
            Enumerable.Range(1, 5).ToList(), 3, 10, 25);

        paginated.HasNextPage.Should().BeFalse();
        paginated.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void PaginatedList_Empty_HasNoPages()
    {
        var paginated = new PaginatedList<int>(
            Enumerable.Empty<int>().ToList(), 1, 10, 0);

        paginated.TotalPages.Should().Be(0);
        paginated.HasNextPage.Should().BeFalse();
        paginated.HasPreviousPage.Should().BeFalse();
    }
}
