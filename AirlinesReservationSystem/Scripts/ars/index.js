$(document).ready(function () {
    var invalidDate = false;
    var invalidSeat = false;

    $('#returnDate').hide();
    $('#roundTripRadio').change(function () {
        if ($('#optionRoundTrip').is(':checked')) {
            $('#returnDate').show();
        }
        else { $('#returnDate').hide(); }
    });


    $('#formDate').change(function () {
        //initialize current date value
        var current = new Date($.now());
        var currentDateEpoch = dateToEpoch(current);
        var currentDate = new Date(currentDateEpoch);

        //get value of dates
        var departureDate = new Date($('#DepartureTime').val());
        var returnDate = new Date($('#ReturnDepartureTime').val());

        //check if date fields are logical, if not then disable the submit button
        if (departureDate > returnDate && $('#optionRoundTrip').is(':checked') || departureDate < currentDate) {
            if (departureDate < currentDate) { $('#dateErrorNow').show(); }
            else { $('#dateError').show(); }
            invalidDate = true;
            console.log(invalidDate);
        }
        else {
            var errorElement = document.getElementById("dateError");
            if (errorElement !== null) {
                $('#dateError').hide();
                $('#dateErrorNow').hide();
                invalidDate = false;
            }
        }
    });

    function dateToEpoch(thedate) {
        return thedate.setHours(0, 0, 0, 0);
    }

    $('#formPassengers').change(function () {
        var adultNo = $('#adultNo').val();
        var childrenNo = $('#childrenNo').val();
        var seniorNo = $('#seniorNo').val();

        if (adultNo < 0) { $('#adultNo').val(0); }
        if (childrenNo < 0) { $('#childrenNo').val(0); }
        if (seniorNo < 0) { $('#seniorNo').val(0); }

        var totalPassenger = adultNo + childrenNo + seniorNo;
        if (totalPassenger <= 0) {
            $('#seatError').show();
            invalidSeat = true;
            console.log(invalidSeat);
        }
        else {
            $('#seatError').hide();
            invalidSeat = false;
        }
    });

    $('#btnSwap').click(function (e) {
        e.preventDefault();
        //get original locations
        var departure = $("#combobox").val();
        var destination = $("#combobox1").val();
        var departureText = $("#combobox option:selected").text();
        var destinationText = $("#combobox1 option:selected").text();
        //console.log(departure + " - " + destination);

        //reverse locations
        $("#combobox").val(destination);
        $("#combobox1").val(departure);

        //reverse location of input fields (which is different from hidden comboboxes)
        var departureInput = document.getElementsByName("autoComplete")[0];
        departureInput.value = destinationText;
        var destinationInput = document.getElementsByName("autoComplete")[1];
        destinationInput.value = departureText;
        //console.log("--> " + $("#combobox").val() + " - " + $("#combobox1").val());
    });

    $('#flightSearchForm').change(function () {
        setTimeout(function () {
            console.log('waiting');
        }, 150);
        if (invalidSeat || invalidDate) {
            $('#btnSubmit').attr('disabled', true);
        }
        else {
            $('#btnSubmit').attr('disabled', false);
        }
    });
});

