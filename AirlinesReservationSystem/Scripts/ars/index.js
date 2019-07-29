﻿$(document).ready(function () {
    $('#returnDate').hide();
    $('#roundTripRadio').change(function () {
        if ($('#optionRoundTrip').is(':checked')) {
            $('#returnDate').show();
        }
        else { $('#returnDate').hide(); }
    });

    
    $('#formDate').change(function () {
        var departureDate = new Date($('#DepartureTime').val());
        var returnDate = new Date($('#ReturnDepartureTime').val());
        if (departureDate > returnDate && $('#optionRoundTrip').is(':checked')) {
            $('#errorMessage').append("<span id='dateError'>Return date can't be earlier than departure date.</span>");
            $("#btnSubmit").attr("disabled", true);
        }
        else {
            var errorElement = document.getElementById("dateError");
            if (errorElement !== null) {
                errorElement.parentNode.removeChild(errorElement);
                $("#btnSubmit").attr("disabled", false);
            }
        }
    });
});

