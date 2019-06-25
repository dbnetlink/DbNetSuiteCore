/// <reference types="jquery" />
/// <reference types="notify" />

class Ajax {
    constructor() {

        $(document).on({
            ajaxStart: function () { $("body").addClass("loading"); },
            ajaxStop: function () { $("body").removeClass("loading"); }
        });

        $.ajaxSetup({
            complete: (xhr, status) => {
                if (xhr.getResponseHeader("X-Responded-JSON") !== null && JSON.parse(xhr.getResponseHeader("X-Responded-JSON")).status === "401") {
                    this.timeout();
                }
            },
            error: (jqxhr, settings, thrownError) => {
                if (jqxhr.status.toString() === "401") { // Login has expired so tell the user and reload the page which will cause a redirect to the login page
                    this.timeout();
                } else {
                    this.ajaxError(jqxhr);
                }
            }
        });
    }

    private timeout() {
        let options: BootboxAlertOptions = {} as BootboxAlertOptions;
        options.message = "Login has timed out";
        options.callback = () => this.reload();
        bootbox.alert(options); 
    }

    private ajaxError(xhr) {
        console.log(JSON.stringify(xhr));

        let options: BootboxAlertOptions = {} as BootboxAlertOptions;
        this.notifyError("Oops! Something went wrong.");
    }

    public callServer(handler: string, data: any, callback: any) {
        let ajaxSettings: JQueryAjaxSettings = {} as JQueryAjaxSettings;
        ajaxSettings.type = "POST";
        ajaxSettings.dataType = "json";
        ajaxSettings.url = "handler.dbnetgrid?handler=" + handler;
        ajaxSettings.data = data;
        ajaxSettings.success = (response) => { callback(response); };
        ajaxSettings.error = (xhr) => {
            this.ajaxError(xhr);
        };
        ajaxSettings.headers = { "RequestVerificationToken": $("body").attr("xsrf-token") };
        $.ajax(ajaxSettings);
    }

    public reload() {
        window.location.reload();
    }

    public notifyInfo(message: string) {
        this.notify(message, "info");
    }

    public notifyError(message: string) {
        this.notify(message, "danger");
    }

    public setAntiForgeryToken() {
        $("input[name='__RequestVerificationToken']").val($("body").attr("xsrf-token"));
    }

    private notify(message: string, className: string) {
        var options = <Notify.Options>{};
        options.globalPosition = "top right";
        options.autoHide = true;
        options.className = className;
        $.notify(message, options)
    }
}
