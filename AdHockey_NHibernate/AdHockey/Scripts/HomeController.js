//report
var report = [];

//declare the module/application
var app = angular.module('Home', ['ngRoute', 'lr.upload']);

//declare the controller
app.controller("HomeController", function ($scope, $http, $q, $rootScope, $route, upload) {
    $scope.isSearch = false;
    $scope.ctReport = undefined;
    $scope.pageNumber = 1;
    $scope.pageSize = 5;
    $scope.searchStr = "";
    $scope.availableTags = [];
    $scope.reportNames = new Array();
    $scope.ctReportPage = new Array();

    $scope.GetReport = function (reportId) {
        $http.get('/Home/GetReport',
            { params: { reportId: reportId } })
        .then(function (response) {
            console.log(response.data);
            $scope.ctReport = response.data;
        });
    }//end method

    $scope.InitializeSearchBox = function () {
        //get json from the database and parse it
        var promise = $http({
            url: '/Home/GetReportNames',
            method: 'GET'
        });

        promise.then(function (response) {
            $scope.availableTags = response.data.$values;

            $('#txtRptSearch').autocomplete({
                source: $scope.availableTags
            });
        });
    }//end method

    $scope.GetReportUnsearchedPage = function () {
        $http.get('/Home/GetReportUnsearchedPage',
            { params: { pageNumber: $scope.pageNumber, pageSize: $scope.pageSize } })
            .then(function (response) {
                $scope.ctReportPage = response.data.$values;
                console.log(response.data);
            });
    }//end method

    $scope.GetReportSearchedPage = function () {
        $http.get('/Home/GetReportSearchedPage',
        { params: { searchStr: $scope.searchStr, pageNumber: $scope.pageNumber, pageSize: $scope.pageSize } })
        .then(function (response) {
            $scope.ctReportPage = response.data.$values;
            console.log(response.data);
        });
    }//end method

    $scope.ReportSelected = function () { 
        //get report id
        var value = $('#chkReport').filter('input:checked').closest('div').find('#hiddenReportId').val();
        var hiddenDiv = $('#hiddenDiv');
        var length = $('#chkReport').filter('input:checked').length;

        if (length > 0) {
            $(hiddenDiv).css('display', '');

            //get report
            $scope.GetReport(new Number(value));
        }
        else {
            $(hiddenDiv).css('display', 'none');
        }
    }//end method

    $scope.SetDataSource = function () {
        try {
            //toggle between searched and unsearched modes
            if ($scope.isSearch)
                $scope.GetReportSearchedPage($scope.searchStr, $scope.pageNumber, $scope.pageSize);
            else
                $scope.GetReportUnsearchedPage($scope.pageNumber, $scope.pageSize);
        }
        catch (err) {
            //set list to empty
            $scope.ctReportPage = [];
            throw err;
        }
    }//end method

    $scope.ExecuteSearch = function (sender) {
        var value = $('#txtRptSearch').val();

        $scope.isSearch = true;
        $scope.searchStr = value;

        $scope.SetDataSource();
    }//end method

    $scope.FirstPage = function () {
        $scope.pageNumber = 1;
        $scope.SetDataSource();
    }//end method

    $scope.PreviousPage = function () {
        if ($scope.pageNumber != 1) {
            $scope.pageNumber--;
            $scope.SetDataSource();
        }
        else
            $scope.SetDataSource();
    }//end method

    $scope.GetReportUnsearchedCount = function () {
        var deferred = $q.defer();
        var promise = $http.get('/Home/GetReportUnsearchedCount');
        return promise;
    }//end method

    $scope.GetReportSearchCount = function () {
        var promise = $http.get('/Home/GetReportSearchCount',
        { params: { searchStr: $scope.searchStr } });
        return promise;
    }//end method

    $scope.NextPage = function () {
        var promise = null;
        if ($scope.isSearch)
            promise = $scope.GetReportSearchCount($scope.searchStr);
        else
            promise = $scope.GetReportUnsearchedCount();

        promise.then(function (response) {
            var numReports = -1;
            console.log(response.data.numReports);
            numReports = response.data.numReports;
            
            if (Math.ceil(numReports / $scope.pageSize) >= $scope.pageNumber + 1) {
                $scope.pageNumber++;
                $scope.SetDataSource();
            }
            else
                $scope.SetDataSource();
        });
    }//end method

    $scope.LastPage = function () {
        //var numReports = -1;
        var promise = null;

        if ($scope.isSearch)
            promise = $scope.GetReportSearchCount($scope.searchStr);
        else
            promise = $scope.GetReportUnsearchedCount();

        promise.then(function (response) {
            var numReports = -1;
            numReports = response.data.numReports;
            
            $scope.pageNumber = Math.floor((numReports / $scope.pageSize) + ((numReports / $scope.pageSize) % 2 == 0 ? 0 : 1))
            $scope.SetDataSource();
        });
    }//end method

    $scope.ctTemplate = null;

    $scope.CallExcelModal = function (ctTemplate) {
        //set the current template we are working on
        $scope.ctTemplate = ctTemplate;

        jQuery.noConflict();

        //call the excel modal
        $('#excelModal').modal();
    }//end method

    $scope.ExcelUpload = function () {
        try {
            console.log("Excel Upload...");
            var file = $("#excelFile")[0].files[0];
            upload({
                url: '/Home/ExcelUpload',
                method: 'POST',
                data: {
                    excelFile: file,
                    jsonBulkTemplate: JSON.stringify($scope.ctTemplate),
                    reportId: $scope.ctReport.ReportId,
                }
            });
        }
        catch(err) {
        } 
        finally {
            console.log("Hide modal...");
            $('#excelModal').modal('hide');
        }
    }//end method

    $scope.ClearSearch = function (control) {
        $scope.isSearch = false;
        $('#txtRptSearch').val('');
        $scope.searchStr = $('#txtRptSearch').val();

        $scope.SetDataSource();
    }//end method

    $scope.index = function () {
        $scope.InitializeSearchBox();
        $scope.SetDataSource();
    }//end method

    $scope.ExecuteReport = function () {
        var json = JSON.stringify($scope.ctReport);
        console.log(json);

        $("#btnSubmit").val(json);
        $("#btnSubmit").click();
    }//end method

    $scope.index();
});