interface DbNetGridResponse
{
    toolbar: string;
    data: string;
    currentPage: number;
    totalPages: number;
    totalRows: number;
    columns: GridColumn[];
    record: Dictionary<String>;
    searchParams: Array<SearchParam>;
    message: string;
    error: boolean;
}

