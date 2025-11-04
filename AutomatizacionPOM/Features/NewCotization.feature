Feature: NewCotization

  Como usuario del sistema
  Quiero poder registrar cotizaciones
  Para gestionar las cotizaciones de clientes

Background:
  Given el usuario ingresa al ambiente 'http://localhost:31096/'
  When el usuario inicia sesión con usuario 'admin@plazafer.com' y contraseña 'calidad'
  And accede al módulo 'Cotizacion'
  And accede al submódulo 'NUEVA COTIZACION'

@RegistrarCotizacion
Scenario Outline: Registro de nueva cotizacion - Casos variados
  When el usuario agrega el concepto '<concepto>'
  And ingresa la cantidad '<cantidad>'
  And <selecciona_igv>
  And selecciona al cliente con documento '<documento>'
  And ingresa la fecha de vencimiento '<fecha_vencimiento>'
  Then <resultado_esperado>

  Examples:
    | caso | concepto    | cantidad | selecciona_igv     | documento      | fecha_vencimiento | resultado_esperado                                                              |
    | 1    | 1010-3      | 2        | selecciona igv     | 00000000       | 01/12/2025        | la cotizacion se guarda correctamente                                           |
    | 2    | 400000437   | 10       | selecciona igv     | 77541694       | 01/12/2025        | la cotizacion se guarda correctamente                                           |
    | 3    | 400000437   | 10       | selecciona igv     | 20172356720    | 01/12/2025        | la cotizacion se guarda correctamente                                           |
    | 4    | 400000437   | 10       | selecciona igv     | UNKNOWN        | 01/12/2025        | la cotizacion se guarda correctamente                                           |
    | 5    | 400000437   | 10       | no selecciona igv  | 00000000       | 01/12/2025        | la cotizacion se guarda correctamente                                           |
    | 6    | 400000437   | 10       | no selecciona igv  | 77541694       | 01/12/2025        | la cotizacion se guarda correctamente                                           |
    | 7    | 400000437   | 10       | no selecciona igv  | 20172356720    | 01/12/2025        | la cotizacion se guarda correctamente                                           |
    | 8    | 400000437   | 10       | no selecciona igv  | UNKNOWN        | 01/12/2025        | la cotizacion se guarda correctamente                                           |
    | 9    | 400000437   | 10       | selecciona igv     | 00000000       | 01/01/2020        | el sistema muestra mensaje de error                                             |
    | 10   | 400000437   | 10       | selecciona igv     | 77541694       | 01/01/2020        | el sistema muestra mensaje de error                                             |
    | 11   | 400000437   | 10       | selecciona igv     | 20172356720    | 01/01/2020        | el sistema muestra mensaje de error                                             |
    | 12   | 400000437   | 10       | selecciona igv     | Unknown        | 01/01/2020        | el sistema muestra mensaje de error                                             |
    | 13   | 400000437   | 10       | no selecciona igv  | 00000000       | 01/01/2020        | el sistema muestra mensaje de error                                             |
    | 14   | 400000437   | 10       | no selecciona igv  | 77541694       | 01/01/2020        | el sistema muestra mensaje de error                                             |
    | 15   | 400000437   | 10       | no selecciona igv  | 20172356720    | 01/01/2020        | el sistema muestra mensaje de error                                             |
    | 16   | 400000437   | 10       | no selecciona igv  | Unknown        | 01/01/2020        | el sistema muestra mensaje de error                                             |
    | 17   | 400000437   | 0        | selecciona igv     | 00000000       | 01/12/2025        | el sistema muestra mensaje: 'Es necesario que el importe total sea mayor a 0.00' |
    | 18   | 400000437   | 0        | selecciona igv     | 77541694       | 01/12/2025        | el sistema muestra mensaje: 'Es necesario que el importe total sea mayor a 0.00' |
    | 19   | 400000437   | 0        | selecciona igv     | 20172356720    | 01/12/2025        | el sistema muestra mensaje: 'Es necesario que el importe total sea mayor a 0.00' |
    | 20   | 400000437   | 0        | selecciona igv     | Unknown        | 01/12/2025        | el sistema muestra mensaje: 'Es necesario que el importe total sea mayor a 0.00' |
    | 21   | 400000437   | 0        | no selecciona igv  | 00000000       | 01/12/2025        | el sistema muestra mensaje: 'Es necesario que el importe total sea mayor a 0.00' |
    | 22   | 400000437   | 0        | no selecciona igv  | 77541694       | 01/12/2025        | el sistema muestra mensaje: 'Es necesario que el importe total sea mayor a 0.00' |
    | 23   | 400000437   | 0        | no selecciona igv  | 20172356720    | 01/12/2025        | el sistema muestra mensaje: 'Es necesario que el importe total sea mayor a 0.00' |
    | 24   | 400000437   | 0        | no selecciona igv  | Unknown        | 01/12/2025        | el sistema muestra mensaje: 'Es necesario que el importe total sea mayor a 0.00' |
    | 25   | 400000437   | 0        | selecciona igv     | 00000000       | 01/01/2020        | el sistema muestra mensaje: 'Es necesario que el importe total sea mayor a 0.00' |
    | 26   | 400000437   | 0        | selecciona igv     | 77541694       | 01/01/2020        | el sistema muestra mensaje: 'Es necesario que el importe total sea mayor a 0.00' |
    | 27   | 400000437   | 0        | selecciona igv     | 20172356720    | 01/01/2020        | el sistema muestra mensaje: 'Es necesario que el importe total sea mayor a 0.00' |
    | 28   | 400000437   | 0        | selecciona igv     | Unknown        | 01/01/2020        | el sistema muestra mensaje: 'Es necesario que el importe total sea mayor a 0.00' |
    | 29   | 400000437   | 0        | no selecciona igv  | 00000000       | 01/01/2020        | el sistema muestra mensaje: 'Es necesario que el importe total sea mayor a 0.00' |
    | 30   | 400000437   | 0        | no selecciona igv  | 77541694       | 01/01/2020        | el sistema muestra mensaje: 'Es necesario que el importe total sea mayor a 0.00' |
    | 31   | 400000437   | 0        | no selecciona igv  | 20172356720    | 01/01/2020        | el sistema muestra mensaje: 'Es necesario que el importe total sea mayor a 0.00' |
    | 32   | 400000437   | 0        | no selecciona igv  | Unknown        | 01/01/2020        | el sistema muestra mensaje: 'Es necesario que el importe total sea mayor a 0.00' |
    | 33   | ninguno        | 10       | selecciona igv     | 00000000       | 01/12/2025        | el sistema muestra mensaje: 'Es necesario seleccionar al menos un producto'      |
    | 34   | ninguno        | 10       | selecciona igv     | 77541694       | 01/12/2025        | el sistema muestra mensaje: 'Es necesario seleccionar al menos un producto'      |
    | 35   | ninguno        | 10       | selecciona igv     | 20172356720    | 01/12/2025        | el sistema muestra mensaje: 'Es necesario seleccionar al menos un producto'      |
    | 36   | ninguno        | 10       | selecciona igv     | Unknown        | 01/12/2025        | el sistema muestra mensaje: 'Es necesario seleccionar al menos un producto'      |
    | 37   | ninguno        | 10       | no selecciona igv  | 00000000       | 01/12/2025        | el sistema muestra mensaje: 'Es necesario seleccionar al menos un producto'      |
    | 38   | ninguno        | 10       | no selecciona igv  | 77541694       | 01/12/2025        | el sistema muestra mensaje: 'Es necesario seleccionar al menos un producto'      |
    | 39   | ninguno        | 10       | no selecciona igv  | 20172356720    | 01/12/2025        | el sistema muestra mensaje: 'Es necesario seleccionar al menos un producto'      |
    | 40   | ninguno        | 10       | no selecciona igv  | Unknown        | 01/12/2025        | el sistema muestra mensaje: 'Es necesario seleccionar al menos un producto'      |
    | 41   | ninguno        | 10       | selecciona igv     | 00000000       | 01/01/2020        | el sistema muestra mensaje: 'Es necesario seleccionar al menos un producto'      |
    | 42   | ninguno        | 10       | selecciona igv     | 77541694       | 01/01/2020        | el sistema muestra mensaje: 'Es necesario seleccionar al menos un producto'      |
    | 43   | ninguno        | 10       | selecciona igv     | 20172356720    | 01/01/2020        | el sistema muestra mensaje: 'Es necesario seleccionar al menos un producto'      |
    | 44   | ninguno        | 10       | selecciona igv     | Unknown        | 01/01/2020        | el sistema muestra mensaje: 'Es necesario seleccionar al menos un producto'      |
    | 45   | ninguno        | 10       | no selecciona igv  | 00000000       | 01/01/2020        | el sistema muestra mensaje: 'Es necesario seleccionar al menos un producto'      |
    | 46   | ninguno        | 10       | no selecciona igv  | 77541694       | 01/01/2020        | el sistema muestra mensaje: 'Es necesario seleccionar al menos un producto'      |
    | 47   | ninguno        | 10       | no selecciona igv  | 20172356720    | 01/01/2020        | el sistema muestra mensaje: 'Es necesario seleccionar al menos un producto'      |
    | 48   | ninguno        | 10       | no selecciona igv  | Unknown        | 01/01/2020        | el sistema muestra mensaje: 'Es necesario seleccionar al menos un producto'      |
    | 49   | ninguno        | 0        | selecciona igv     | 00000000       | 01/12/2025        | el sistema muestra mensaje: 'Es necesario que el importe total sea mayor a 0.00' |
    | 50   | ninguno        | 0        | selecciona igv     | 77541694       | 01/12/2025        | el sistema muestra mensaje: 'Es necesario que el importe total sea mayor a 0.00' |
    | 51   | ninguno        | 0        | selecciona igv     | 20172356720    | 01/12/2025        | el sistema muestra mensaje: 'Es necesario que el importe total sea mayor a 0.00' |
    | 52   | ninguno        | 0        | selecciona igv     | Unknown        | 01/12/2025        | el sistema muestra mensaje: 'Es necesario que el importe total sea mayor a 0.00' |
    | 53   | ninguno        | 0        | no selecciona igv  | 00000000       | 01/12/2025        | el sistema muestra mensaje: 'Es necesario que el importe total sea mayor a 0.00' |
    | 54   | ninguno        | 0        | no selecciona igv  | 77541694       | 01/12/2025        | el sistema muestra mensaje: 'Es necesario que el importe total sea mayor a 0.00' |
    | 55   | ninguno        | 0        | no selecciona igv  | 20172356720    | 01/12/2025        | el sistema muestra mensaje: 'Es necesario que el importe total sea mayor a 0.00' |
    | 56   | ninguno        | 0        | no selecciona igv  | Unknown        | 01/12/2025        | el sistema muestra mensaje: 'Es necesario que el importe total sea mayor a 0.00' |
    | 57   | ninguno        | 0        | selecciona igv     | 00000000       | 01/01/2020        | el sistema muestra mensaje: 'Es necesario que el importe total sea mayor a 0.00' |
    | 58   | 400000437   | 0        | selecciona igv     | 77541694       | 01/01/2020        | el sistema muestra mensaje: 'Es necesario que el importe total sea mayor a 0.00' |
    | 59   | ninguno        | 0        | selecciona igv     | 20172356720    | 01/01/2020        | el sistema muestra mensaje: 'Es necesario que el importe total sea mayor a 0.00' |
    | 60   | ninguno        | 0        | selecciona igv     | Unknown        | 01/01/2020        | el sistema muestra mensaje: 'Es necesario que el importe total sea mayor a 0.00' |
    | 61   | ninguno        | 0        | no selecciona igv  | 00000000       | 01/01/2020        | el sistema muestra mensaje: 'Es necesario que el importe total sea mayor a 0.00' |
    | 62   | ninguno        | 0        | no selecciona igv  | 77541694       | 01/01/2020        | el sistema muestra mensaje: 'Es necesario que el importe total sea mayor a 0.00' |
    | 63   | ninguno        | 0        | no selecciona igv  | 20172356720    | 01/01/2020        | el sistema muestra mensaje: 'Es necesario que el importe total sea mayor a 0.00' |
    | 64   | ninguno        | 0        | no selecciona igv  | Unknown        | 01/01/2020        | el sistema muestra mensaje: 'Es necesario que el importe total sea mayor a 0.00' |
