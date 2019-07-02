/// <reference types="jquery" />
/// <reference types="bootstrap" />
/// <reference types="bootbox" />
/// <reference path="Ajax.ts" />
/// <reference path="Interfaces.ts" />

class DbNetGrid extends Ajax {
    configuration: DbNetGridConfiguration;
    $container: JQuery<HTMLElement>

    constructor(configuration : DbNetGridConfiguration) {
        super();
        this.configuration = configuration;
        this.init();
    }

    private init() {
        this.callServer("Init", this.configuration, (response) => { this.initCallback(response) });
    }

    private initCallback(response: DbNetGridConfiguration) {
        this.$container = $(`#${this.configuration.id}`)
        this.$container.html(response.html.toolbar);
        this.$container.find(".table-container").html(response.html.page);
    }
}