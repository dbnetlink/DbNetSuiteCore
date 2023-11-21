interface SearchParam
{
    columnIndex?: string | undefined;
    columnType?: string | undefined;
    searchOperator: string;
    value1: string;
    value2: string;
    value1Valid?: boolean;
    value2Valid?: boolean;
    unit1?: string;
    unit2?: string;
}