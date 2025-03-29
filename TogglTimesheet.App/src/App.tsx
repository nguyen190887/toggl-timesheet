import React, { useState, useEffect } from 'react';
import TextField from '@mui/material/TextField';
import Button from '@mui/material/Button';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';

const App: React.FC = () => {
  const [startDate, setStartDate] = useState<string>('');
  const [endDate, setEndDate] = useState<string>('');

  useEffect(() => {
    const today = new Date();
    const closestSunday = new Date(today);
    closestSunday.setDate(today.getDate() - today.getDay()); // Set to the closest Sunday in the past

    const closestSundayISO = closestSunday.toISOString().split('T')[0];
    setEndDate(closestSundayISO);

    const startDate = new Date(closestSunday);
    startDate.setDate(closestSunday.getDate() - 6); // 6 days before the closest Sunday
    const startDateISO = startDate.toISOString().split('T')[0];
    setStartDate(startDateISO);
  }, []);

  const handleGetTimeReport = () => {
    alert(`Fetching time report from ${startDate} to ${endDate}`);
  };

  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        gap: 2,
        maxWidth: 400,
        margin: 'auto',
        padding: 2,
        borderRadius: 2,
        boxShadow: 3,
      }}
    >
      <Typography variant="h4" component="h1" gutterBottom>
        Timesheet Report App
      </Typography>
      <TextField
        label="Start Date"
        type="date"
        value={startDate}
        onChange={(e) => setStartDate(e.target.value)}
        InputLabelProps={{ shrink: true }}
      />
      <TextField
        label="End Date"
        type="date"
        value={endDate}
        onChange={(e) => setEndDate(e.target.value)}
        InputLabelProps={{ shrink: true }}
      />
      <Button
        variant="contained"
        color="primary"
        onClick={handleGetTimeReport}
        sx={{ mt: 2 }}
      >
        Generate Report
      </Button>
    </Box>
  );
};

export default App;
