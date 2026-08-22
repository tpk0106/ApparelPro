/* =====================================================================
   Part Shipment data load - one shipment for LF/1017-18/MJKTS/ANCHORAGE
   (OD_PART.DBF -> PartShipments)

   Source: C:\ap-fe\apparelpro-front-end-react\clipper-migration\references\
   OD_PART.DBF (57 records, no CSV export existed - read directly from the
   binary DBF). Exactly one record matched Buyer=LF, Order=1017-18,
   Style=ANCHORAGE:

     NEW_ORDER=PONO.1017-18  DEST=BAL  UNIT=PCS  QTY=2500.00  BALANCE=2000.00
     SHIP_DATE=941215 (1994-12-15)  SHP_MODE=SEA  SUBCONT=N
     QTA_STAT=Q  QTA_CAT=334  QTA_TYPE=OR  QTA_CONT=USA
     F_YYMM=94/07  T_YYMM=95/06  ODATE=950201 (1995-02-01)

   Only FK on this table is Unit -> Units.Code; PCS already exists (Phase 1
   seed). DestinationCode has no FK (Destinations.Id is an int surrogate
   key, unrelated to this 3-char legacy code), so 'BAL' inserts as plain data.

   Idempotent: safe to re-run - only inserts if this exact key doesn't
   already exist.
   ===================================================================== */

SET NOCOUNT ON;

INSERT INTO dbo.PartShipments
    (BuyerCode, [Order], TypeCode, StyleCode, NewOrder, DestinationCode, ShipDate,
     SubContractFlag, Unit, Quantity, ShippingMode, QuotaCountry, QuotaStatus,
     QuotaCategory, QuotaType, FromYearMonth, ToYearMonth, OrderDate, Balance)
SELECT v.BuyerCode, v.[Order], v.TypeCode, v.StyleCode, v.NewOrder, v.DestinationCode, v.ShipDate,
       v.SubContractFlag, v.Unit, v.Quantity, v.ShippingMode, v.QuotaCountry, v.QuotaStatus,
       v.QuotaCategory, v.QuotaType, v.FromYearMonth, v.ToYearMonth, v.OrderDate, v.Balance
FROM (VALUES
    (1, N'1017-18', 2, N'ANCHORAGE', N'PONO.1017-18', N'BAL', CAST('1994-12-15' AS date),
     N'N', N'PCS', 2500.00, N'SEA', N'USA', N'Q',
     N'334', N'OR', N'94/07', N'95/06', CAST('1995-02-01' AS date), 2000.00)
) AS v(BuyerCode, [Order], TypeCode, StyleCode, NewOrder, DestinationCode, ShipDate,
       SubContractFlag, Unit, Quantity, ShippingMode, QuotaCountry, QuotaStatus,
       QuotaCategory, QuotaType, FromYearMonth, ToYearMonth, OrderDate, Balance)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.PartShipments p
    WHERE p.BuyerCode = v.BuyerCode AND p.[Order] = v.[Order]
      AND p.TypeCode = v.TypeCode AND p.StyleCode = v.StyleCode
      AND p.NewOrder = v.NewOrder
);

SELECT * FROM dbo.PartShipments
WHERE BuyerCode = 1 AND [Order] = '1017-18' AND TypeCode = 2 AND StyleCode = 'ANCHORAGE';
