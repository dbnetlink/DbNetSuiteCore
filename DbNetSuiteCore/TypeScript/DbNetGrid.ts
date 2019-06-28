/// <reference types="jquery" />
/// <reference types="bootstrap" />
/// <reference types="bootbox" />
/// <reference path="Ajax.ts" />
/// <reference path="Interfaces.ts" />


class DbNetGrid extends Ajax {
    configuration: DbNetGridConfiguration;

    constructor(configuration : DbNetGridConfiguration) {
        super();
        this.configuration = configuration;
        this.init();
    }

    private init() {
        this.callServer("Init", this.configuration, (response) => { this.initCallback(response) });
    }

    private initCallback(response: DbNetGridConfiguration) {
        alert(response.tableName);
    }
}