interface DbNetFileResponse extends DbNetSuiteResponse
{
    currentPage: number;
    totalPages: number;
    totalRows: number;
    searchParams?: Array<SearchParam>;
}